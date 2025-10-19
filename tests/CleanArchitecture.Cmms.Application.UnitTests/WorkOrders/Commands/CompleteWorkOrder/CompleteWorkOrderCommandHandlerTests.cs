using CleanArchitecture.Cmms.Application.Abstractions.Common;
using CleanArchitecture.Cmms.Application.Abstractions.Persistence.Repositories;
using CleanArchitecture.Cmms.Application.WorkOrders.Commands.CompleteWorkOrder;
using CleanArchitecture.Cmms.Domain.Technicians;
using CleanArchitecture.Cmms.Domain.Technicians.ValueObjects;
using CleanArchitecture.Cmms.Domain.WorkOrders;
using CleanArchitecture.Cmms.Domain.WorkOrders.ValueObjects;

namespace CleanArchitecture.Cmms.Application.UnitTests.WorkOrders.Commands.CompleteWorkOrder;

public class CompleteWorkOrderCommandHandlerTests
{
    private readonly Mock<IRepository<WorkOrder, Guid>> _workOrderRepositoryMock;
    private readonly Mock<IRepository<Technician, Guid>> _technicianRepositoryMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly CompleteWorkOrderCommandHandler _sut;

    public CompleteWorkOrderCommandHandlerTests()
    {
        _workOrderRepositoryMock = new Mock<IRepository<WorkOrder, Guid>>();
        _technicianRepositoryMock = new Mock<IRepository<Technician, Guid>>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        _sut = new CompleteWorkOrderCommandHandler(
            _workOrderRepositoryMock.Object,
            _technicianRepositoryMock.Object,
            _dateTimeProviderMock.Object);
    }

    [Fact]
    public async Task Handle_WhenWorkOrderAndTechnicianExist_ShouldCompleteWorkOrder()
    {
        // Arrange
        var technician = CreateTechnician();
        var workOrder = CreateWorkOrderWithTechnician(technician.Id);
        technician.AddAssignedOrder(workOrder.Id, DateTime.UtcNow);
        var utcNow = DateTime.UtcNow;
        var command = new CompleteWorkOrderCommand(workOrder.Id);

        _workOrderRepositoryMock.Setup(x => x.GetByIdAsync(workOrder.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workOrder);

        _technicianRepositoryMock.Setup(x => x.GetByIdAsync(technician.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(technician);

        _dateTimeProviderMock.Setup(x => x.UtcNow).Returns(utcNow);

        _workOrderRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<WorkOrder>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _technicianRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Technician>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _workOrderRepositoryMock.Verify(x => x.GetByIdAsync(workOrder.Id, It.IsAny<CancellationToken>()), Times.Once);
        _technicianRepositoryMock.Verify(x => x.GetByIdAsync(technician.Id, It.IsAny<CancellationToken>()), Times.Once);
        _workOrderRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<WorkOrder>(), It.IsAny<CancellationToken>()), Times.Once);
        _technicianRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Technician>(), It.IsAny<CancellationToken>()), Times.Once);
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
        _technicianRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenWorkOrderHasNoTechnician_ShouldReturnFailure()
    {
        // Arrange
        var workOrder = CreateWorkOrderWithoutTechnician();
        var command = new CompleteWorkOrderCommand(workOrder.Id);

        _workOrderRepositoryMock.Setup(x => x.GetByIdAsync(workOrder.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workOrder);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(Application.Technicians.TechnicianErrors.NotFound.Code);
        result.Error.Message.Should().Be(Application.Technicians.TechnicianErrors.NotFound.Message);
    }

    [Fact]
    public async Task Handle_WhenTechnicianNotFound_ShouldReturnFailure()
    {
        // Arrange
        var technician = CreateTechnician();
        var workOrder = CreateWorkOrderWithTechnician(technician.Id);
        var command = new CompleteWorkOrderCommand(workOrder.Id);

        _workOrderRepositoryMock.Setup(x => x.GetByIdAsync(workOrder.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workOrder);

        _technicianRepositoryMock.Setup(x => x.GetByIdAsync(technician.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Technician?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(Application.Technicians.TechnicianErrors.NotFound.Code);
        result.Error.Message.Should().Be(Application.Technicians.TechnicianErrors.NotFound.Message);

        _workOrderRepositoryMock.Verify(x => x.GetByIdAsync(workOrder.Id, It.IsAny<CancellationToken>()), Times.Once);
        _technicianRepositoryMock.Verify(x => x.GetByIdAsync(technician.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    private static Technician CreateTechnician()
    {
        var skillLevel = SkillLevel.Apprentice;
        var technician = Technician.Create("John Doe", skillLevel);
        return technician;
    }

    private static WorkOrder CreateWorkOrderWithTechnician(Guid technicianId)
    {
        var location = Location.Create("Building A", "Floor 1", "Room 101");
        var workOrder = WorkOrder.Create(Guid.NewGuid(), "Test Work Order", location);
        workOrder.AssignTechnician(technicianId);
        workOrder.Start();
        return workOrder;
    }

    private static WorkOrder CreateWorkOrderWithoutTechnician()
    {
        var location = Location.Create("Building A", "Floor 1", "Room 101");

        var workOrder = WorkOrder.Create(Guid.NewGuid(), "Test Work Order", location);

        workOrder.AssignTechnician(Guid.NewGuid());

        workOrder.Start();

        return workOrder;
    }
}
