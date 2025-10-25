using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;
using CleanArchitecture.Cmms.Application.WorkOrders.Commands.StartWorkOrder;
using CleanArchitecture.Cmms.Domain.WorkOrders;
using CleanArchitecture.Cmms.Domain.WorkOrders.ValueObjects;

namespace CleanArchitecture.Cmms.Application.UnitTests.WorkOrders.Commands.StartWorkOrder;

public class StartWorkOrderCommandHandlerTests
{
    private readonly Mock<IRepository<WorkOrder, Guid>> _repositoryMock;
    private readonly StartWorkOrderCommandHandler _sut;

    public StartWorkOrderCommandHandlerTests()
    {
        _repositoryMock = new Mock<IRepository<WorkOrder, Guid>>();
        _sut = new StartWorkOrderCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenWorkOrderExists_ShouldStartWorkOrder()
    {
        // Arrange
        var assetId = Guid.NewGuid();
        var workOrder = CreateMockWorkOrder(assetId);
        var workOrderId = workOrder.Id;
        var command = new StartWorkOrderCommand(workOrderId);

        _repositoryMock.Setup(x => x.GetByIdAsync(workOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workOrder);

        _repositoryMock.Setup(x => x.UpdateAsync(It.IsAny<WorkOrder>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _repositoryMock.Verify(x => x.GetByIdAsync(workOrderId, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.UpdateAsync(workOrder, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenWorkOrderNotFound_ShouldReturnFailure()
    {
        // Arrange
        var workOrderId = Guid.NewGuid();
        var command = new StartWorkOrderCommand(workOrderId);

        _repositoryMock.Setup(x => x.GetByIdAsync(workOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkOrder?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(Application.WorkOrders.WorkOrderErrors.NotFound.Code);
        result.Error.Message.Should().Be(Application.WorkOrders.WorkOrderErrors.NotFound.Message);

        _repositoryMock.Verify(x => x.GetByIdAsync(workOrderId, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<WorkOrder>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static WorkOrder CreateMockWorkOrder(Guid id)
    {
        var location = Location.Create("SiteA", "AreaB", "ZoneC");

        var workOrder = WorkOrder.Create(id, "Order 1", location);

        workOrder.AssignTechnician(Guid.NewGuid());

        return workOrder;
    }
}
