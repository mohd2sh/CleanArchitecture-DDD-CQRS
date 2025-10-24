using CleanArchitecture.Cmms.Application.WorkOrders.EventsHandlers;
using CleanArchitecture.Cmms.Domain.WorkOrders.Events;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Cmms.Application.UnitTests.WorkOrders.EventsHandlers.WorkOrderCreated;

public class WorkOrderCreatedEventHandlerTests
{
    private readonly Mock<ILogger<WorkOrderCreatedEventHandler>> _loggerMock;
    private readonly WorkOrderCreatedEventHandler _sut;

    public WorkOrderCreatedEventHandlerTests()
    {
        _loggerMock = new Mock<ILogger<WorkOrderCreatedEventHandler>>();
        _sut = new WorkOrderCreatedEventHandler(_loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldLogWorkOrderCreatedEvent()
    {
        // Arrange
        var workOrderId = Guid.NewGuid();
        var assetId = Guid.NewGuid();

        var workOrderCreatedEvent = new WorkOrderCreatedEvent(workOrderId, assetId, "Order 1");

        // Act
        await _sut.Handle(workOrderCreatedEvent, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Work Order Created Event Handled: {workOrderId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithDifferentWorkOrderId_ShouldLogCorrectId()
    {
        // Arrange
        var workOrderId = Guid.NewGuid();
        var assetId = Guid.NewGuid();
        var workOrderCreatedEvent = new WorkOrderCreatedEvent(workOrderId, assetId, "Title");

        // Act
        await _sut.Handle(workOrderCreatedEvent, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Work Order Created Event Handled: {workOrderId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
