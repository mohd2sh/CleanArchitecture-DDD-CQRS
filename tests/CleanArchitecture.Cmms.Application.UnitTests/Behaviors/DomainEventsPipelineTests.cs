using CleanArchitecture.Cmms.Application.Behaviors;
using CleanArchitecture.Core.Application.Abstractions.Events;
using CleanArchitecture.Core.Application.Abstractions.Messaging;
using CleanArchitecture.Core.Application.Abstractions.Persistence;
using CleanArchitecture.Core.Domain.Abstractions;
using CleanArchitecture.Outbox.Abstractions;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Cmms.Application.UnitTests.Behaviors;

public class DomainEventsPipelineTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IDomainEventDispatcher> _eventDispatcherMock;
    private readonly Mock<IOutboxStore> _outboxStoreMock;
    private readonly Mock<ILogger<DomainEventsPipeline<TestCommand, TestResponse>>> _loggerMock;
    private readonly DomainEventsPipeline<TestCommand, TestResponse> _sut;

    public DomainEventsPipelineTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _eventDispatcherMock = new Mock<IDomainEventDispatcher>();
        _outboxStoreMock = new Mock<IOutboxStore>();
        _loggerMock = new Mock<ILogger<DomainEventsPipeline<TestCommand, TestResponse>>>();

        _sut = new DomainEventsPipeline<TestCommand, TestResponse>(
            _unitOfWorkMock.Object,
            _eventDispatcherMock.Object,
            _outboxStoreMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenNoDomainEvents_ShouldReturnResultWithoutProcessing()
    {
        // Arrange
        var command = new TestCommand { Id = 123 };
        var expectedResponse = new TestResponse { Result = "Success" };

        _unitOfWorkMock.Setup(x => x.CollectDomainEvents())
            .Returns(new List<IDomainEvent>());

        // Act
        var result = await _sut.Handle(command, () => Task.FromResult(expectedResponse), CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
        _unitOfWorkMock.Verify(x => x.CollectDomainEvents(), Times.Once);
        _eventDispatcherMock.Verify(x => x.PublishAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _outboxStoreMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenDomainEventExists_ShouldPublishToTransactionalAndWriteToOutbox()
    {
        // Arrange
        var command = new TestCommand { Id = 123 };
        var expectedResponse = new TestResponse { Result = "Success" };
        var domainEvent = new TestDomainEvent();

        _unitOfWorkMock.SetupSequence(x => x.CollectDomainEvents())
            .Returns(new List<IDomainEvent> { domainEvent })
            .Returns(new List<IDomainEvent>());

        // Act
        var result = await _sut.Handle(command, () => Task.FromResult(expectedResponse), CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
        // Every event is published to transactional pipeline
        _eventDispatcherMock.Verify(x => x.PublishAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        // Every event is added to outbox
        _outboxStoreMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNestedEvents_ShouldProcessInBatches()
    {
        // Arrange
        var command = new TestCommand { Id = 123 };
        var expectedResponse = new TestResponse { Result = "Success" };
        var firstEvent = new TestDomainEvent();
        var nestedEvent = new TestDomainEvent();

        // Simulate handler raising a new event
        _unitOfWorkMock.SetupSequence(x => x.CollectDomainEvents())
            .Returns(new List<IDomainEvent> { firstEvent })  // Batch 1
            .Returns(new List<IDomainEvent> { nestedEvent }) // Batch 2 (raised by handler)
            .Returns(new List<IDomainEvent>());              // No more events

        // Act
        var result = await _sut.Handle(command, () => Task.FromResult(expectedResponse), CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
        _unitOfWorkMock.Verify(x => x.CollectDomainEvents(), Times.Exactly(3)); // Called until empty
        // Both events published to transactional pipeline
        _eventDispatcherMock.Verify(x => x.PublishAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        // Both events added to outbox
        _outboxStoreMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_WhenTransactionalHandlerFails_ShouldNotWriteToOutbox()
    {
        // Arrange
        var command = new TestCommand { Id = 123 };
        var expectedResponse = new TestResponse { Result = "Success" };
        var domainEvent = new TestDomainEvent();

        _unitOfWorkMock.SetupSequence(x => x.CollectDomainEvents())
            .Returns(new List<IDomainEvent> { domainEvent })
            .Returns(new List<IDomainEvent>());

        _eventDispatcherMock.Setup(x => x.PublishAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Handler failed"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.Handle(command, () => Task.FromResult(expectedResponse), CancellationToken.None));

        // Outbox should NOT be written because exception occurred before WriteIntegrationEventsToOutbox
        _outboxStoreMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenMultipleBatchesWithEvents_ShouldProcessCorrectly()
    {
        // Arrange
        var command = new TestCommand { Id = 123 };
        var expectedResponse = new TestResponse { Result = "Success" };

        _unitOfWorkMock.SetupSequence(x => x.CollectDomainEvents())
            .Returns(new List<IDomainEvent>
            {
                new TestDomainEvent(),
                new TestDomainEvent(),
                new TestDomainEvent()
            })
            .Returns(new List<IDomainEvent>
            {
                new TestDomainEvent(),
                new TestDomainEvent()
            })
            .Returns(new List<IDomainEvent>());

        // Act
        var result = await _sut.Handle(command, () => Task.FromResult(expectedResponse), CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);

        // 3 events from batch 1 + 2 from batch 2 = 5 publishes
        _eventDispatcherMock.Verify(x => x.PublishAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Exactly(5));

        // 3 events from batch 1 + 2 from batch 2 = 5 outbox writes
        _outboxStoreMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(5));
    }

    [Fact]
    public async Task Handle_WhenNextThrowsException_ShouldNotProcessEvents()
    {
        // Arrange
        var command = new TestCommand { Id = 123 };
        var expectedException = new InvalidOperationException("Command failed");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.Handle(command, () => throw expectedException, CancellationToken.None));

        exception.Should().Be(expectedException);
        _unitOfWorkMock.Verify(x => x.CollectDomainEvents(), Times.Never);
        _eventDispatcherMock.Verify(x => x.PublishAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _outboxStoreMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // Test helper classes
    public class TestCommand : ICommand<TestResponse>
    {
        public int Id { get; set; }
    }

    public class TestResponse
    {
        public string Result { get; set; } = string.Empty;
    }

    public record TestDomainEvent : IDomainEvent
    {
        public DateTime? OccurredOn { get; } = DateTime.UtcNow;
    }
}
