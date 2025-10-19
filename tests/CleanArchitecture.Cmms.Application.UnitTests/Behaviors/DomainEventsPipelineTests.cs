using CleanArchitecture.Cmms.Application.Abstractions.Messaging;
using CleanArchitecture.Cmms.Application.Abstractions.Persistence;
using CleanArchitecture.Cmms.Application.Behaviors;
using CleanArchitecture.Cmms.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Application.UnitTests.Behaviors;

public class DomainEventsPipelineTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly DomainEventsPipeline<TestCommand, TestResponse> _sut;

    public DomainEventsPipelineTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mediatorMock = new Mock<IMediator>();
        _sut = new DomainEventsPipeline<TestCommand, TestResponse>(_unitOfWorkMock.Object, _mediatorMock.Object);
    }

    [Fact]
    public async Task Handle_WhenNoDomainEvents_ShouldReturnResultWithoutPublishing()
    {
        // Arrange
        var command = new TestCommand { Id = 123 };
        var expectedResponse = new TestResponse { Result = "Success" };
        var domainEvents = new List<IDomainEvent>();

        _unitOfWorkMock.Setup(x => x.CollectDomainEvents()).Returns(domainEvents);

        // Act
        var result = await _sut.Handle(command, () => Task.FromResult(expectedResponse), CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);

        _unitOfWorkMock.Verify(x => x.CollectDomainEvents(), Times.Once);
        _mediatorMock.Verify(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenDomainEventsExist_ShouldPublishAllEvents()
    {
        // Arrange
        var command = new TestCommand { Id = 123 };
        var expectedResponse = new TestResponse { Result = "Success" };
        var domainEvents = new List<IDomainEvent>
        {
            new Mock<IDomainEvent>().Object,
            new Mock<IDomainEvent>().Object
        };

        _unitOfWorkMock.Setup(x => x.CollectDomainEvents()).Returns(domainEvents);

        // Act
        var result = await _sut.Handle(command, () => Task.FromResult(expectedResponse), CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);

        _unitOfWorkMock.Verify(x => x.CollectDomainEvents(), Times.Once);
        _mediatorMock.Verify(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_WhenMultipleDomainEvents_ShouldPublishEachEvent()
    {
        // Arrange
        var command = new TestCommand { Id = 123 };
        var expectedResponse = new TestResponse { Result = "Success" };
        var domainEvent1 = new Mock<IDomainEvent>();
        var domainEvent2 = new Mock<IDomainEvent>();
        var domainEvent3 = new Mock<IDomainEvent>();
        var domainEvents = new List<IDomainEvent> { domainEvent1.Object, domainEvent2.Object, domainEvent3.Object };

        _unitOfWorkMock.Setup(x => x.CollectDomainEvents()).Returns(domainEvents);

        _mediatorMock.Setup(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(command, () => Task.FromResult(expectedResponse), CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);

        _unitOfWorkMock.Verify(x => x.CollectDomainEvents(), Times.Once);
        _mediatorMock.Verify(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public async Task Handle_WhenNextThrowsException_ShouldNotPublishEvents()
    {
        // Arrange
        var command = new TestCommand { Id = 123 };
        var domainEvents = new List<IDomainEvent> { new Mock<IDomainEvent>().Object };
        var expectedException = new InvalidOperationException("Test exception");

        _unitOfWorkMock.Setup(x => x.CollectDomainEvents()).Returns(domainEvents);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.Handle(command, () => throw expectedException, CancellationToken.None));

        exception.Should().Be(expectedException);

        _unitOfWorkMock.Verify(x => x.CollectDomainEvents(), Times.Never);
        _mediatorMock.Verify(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    public class TestCommand : ICommand<TestResponse>
    {
        public int Id { get; set; }
    }

    public class TestResponse
    {
        public string Result { get; set; } = string.Empty;
    }
}
