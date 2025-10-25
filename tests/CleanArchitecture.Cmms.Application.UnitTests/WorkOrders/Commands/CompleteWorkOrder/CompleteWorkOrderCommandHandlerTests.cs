using CleanArchitecture.Cmms.Application.Abstractions.Persistence.Repositories;
using CleanArchitecture.Cmms.Application.WorkOrders.Commands.CompleteWorkOrder;
using CleanArchitecture.Cmms.Domain.WorkOrders;
using CleanArchitecture.Cmms.Domain.WorkOrders.ValueObjects;

namespace CleanArchitecture.Cmms.Application.UnitTests.WorkOrders.Commands.CompleteWorkOrder;

public class CompleteWorkOrderCommandHandlerTests
{
    private readonly Mock<IRepository<WorkOrder, Guid>> _workOrderRepositoryMock;
    private readonly CompleteWorkOrderCommandHandler _sut;

    public CompleteWorkOrderCommandHandlerTests()
    {
        _workOrderRepositoryMock = new Mock<IRepository<WorkOrder, Guid>>();
        _sut = new CompleteWorkOrderCommandHandler(_workOrderRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenWorkOrderExists_ShouldCompleteWorkOrder()
    {
        // Arrange
        var workOrder = CreateWorkOrderWithTechnician(Guid.NewGuid());
        var command = new CompleteWorkOrderCommand(workOrder.Id);

        _workOrderRepositoryMock.Setup(x => x.GetByIdAsync(workOrder.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workOrder);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _workOrderRepositoryMock.Verify(x => x.GetByIdAsync(workOrder.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenWorkOrderNotFound_ShouldReturnFailure()
    {
        // Arrange
        var workOrderId = Guid.NewGuid();
        var command = new CompleteWorkOrderCommand(workOrderId);

        _workOrderRepositoryMock.Setup(x => x.GetByIdAsync(workOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkOrder?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(Application.WorkOrders.WorkOrderErrors.NotFound.Code);
        result.Error.Message.Should().Be(Application.WorkOrders.WorkOrderErrors.NotFound.Message);

        _workOrderRepositoryMock.Verify(x => x.GetByIdAsync(workOrderId, It.IsAny<CancellationToken>()), Times.Once);
    }

    private static WorkOrder CreateWorkOrderWithTechnician(Guid technicianId)
    {
        var location = Location.Create("Building A", "Floor 1", "Room 101");
        var workOrder = WorkOrder.Create(Guid.NewGuid(), "Test Work Order", location);
        workOrder.AssignTechnician(technicianId);
        workOrder.Start();
        return workOrder;
    }
}
