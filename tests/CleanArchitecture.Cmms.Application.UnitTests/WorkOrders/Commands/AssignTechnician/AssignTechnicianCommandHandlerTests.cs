using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;
using CleanArchitecture.Cmms.Application.WorkOrders.Commands.AssignTechnician;
using CleanArchitecture.Cmms.Domain.WorkOrders;
using CleanArchitecture.Cmms.Domain.WorkOrders.ValueObjects;

namespace CleanArchitecture.Cmms.Application.UnitTests.WorkOrders.Commands.AssignTechnician;

public class AssignTechnicianCommandHandlerTests
{
    private readonly Mock<IRepository<WorkOrder, Guid>> _workOrderRepositoryMock;
    private readonly AssignTechnicianCommandHandler _sut;

    public AssignTechnicianCommandHandlerTests()
    {
        _workOrderRepositoryMock = new Mock<IRepository<WorkOrder, Guid>>();
        _sut = new AssignTechnicianCommandHandler(_workOrderRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenWorkOrderExists_ShouldAssignTechnician()
    {
        // Arrange
        var workOrder = CreateWorkOrder();
        var technicianId = Guid.NewGuid();
        var command = new AssignTechnicianCommand(workOrder.Id, technicianId);

        _workOrderRepositoryMock.Setup(x => x.GetByIdAsync(workOrder.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workOrder);

        _workOrderRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<WorkOrder>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _workOrderRepositoryMock.Verify(x => x.GetByIdAsync(workOrder.Id, It.IsAny<CancellationToken>()), Times.Once);
        _workOrderRepositoryMock.Verify(x => x.UpdateAsync(workOrder, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenWorkOrderExists_ShouldAssignTechnicianWithCorrectId()
    {
        // Arrange
        var workOrder = CreateWorkOrder();
        var technicianId = Guid.NewGuid();
        var command = new AssignTechnicianCommand(workOrder.Id, technicianId);

        _workOrderRepositoryMock.Setup(x => x.GetByIdAsync(workOrder.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workOrder);

        WorkOrder? capturedWorkOrder = null;
        _workOrderRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<WorkOrder>(), It.IsAny<CancellationToken>()))
            .Callback<WorkOrder, CancellationToken>((wo, ct) => capturedWorkOrder = wo)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedWorkOrder.Should().NotBeNull();
        capturedWorkOrder!.TechnicianId.Should().Be(technicianId);
    }

    [Fact]
    public async Task Handle_WhenWorkOrderNotFound_ShouldReturnFailure()
    {
        // Arrange
        var workOrderId = Guid.NewGuid();
        var technicianId = Guid.NewGuid();
        var command = new AssignTechnicianCommand(workOrderId, technicianId);

        _workOrderRepositoryMock.Setup(x => x.GetByIdAsync(workOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkOrder?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Be("Work order not found.");
        result.Error.Type.Should().Be(ErrorType.NotFound);

        _workOrderRepositoryMock.Verify(x => x.GetByIdAsync(workOrderId, It.IsAny<CancellationToken>()), Times.Once);
        _workOrderRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<WorkOrder>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
    {
        // Arrange
        var workOrder = CreateWorkOrder();
        var technicianId = Guid.NewGuid();
        var command = new AssignTechnicianCommand(workOrder.Id, technicianId);

        var expectedException = new InvalidOperationException("Database error");
        _workOrderRepositoryMock.Setup(x => x.GetByIdAsync(workOrder.Id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.Handle(command, CancellationToken.None));

        exception.Should().Be(expectedException);
    }

    [Fact]
    public async Task Handle_WhenUpdateThrows_ShouldPropagateException()
    {
        // Arrange
        var workOrder = CreateWorkOrder();
        var technicianId = Guid.NewGuid();
        var command = new AssignTechnicianCommand(workOrder.Id, technicianId);

        _workOrderRepositoryMock.Setup(x => x.GetByIdAsync(workOrder.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workOrder);

        var expectedException = new InvalidOperationException("Update failed");
        _workOrderRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<WorkOrder>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.Handle(command, CancellationToken.None));

        exception.Should().Be(expectedException);
    }

    private static WorkOrder CreateWorkOrder()
    {
        var location = Location.Create("Building A", "Floor 1", "Room 101");
        var workOrder = WorkOrder.Create(Guid.NewGuid(), "Test Work Order", location);
        return workOrder;
    }
}
