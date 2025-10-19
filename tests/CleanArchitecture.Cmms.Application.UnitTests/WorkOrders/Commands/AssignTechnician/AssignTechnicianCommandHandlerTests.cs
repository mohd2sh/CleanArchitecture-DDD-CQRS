using CleanArchitecture.Cmms.Application.Abstractions.Common;
using CleanArchitecture.Cmms.Application.Abstractions.Persistence.Repositories;
using CleanArchitecture.Cmms.Application.Primitives;
using CleanArchitecture.Cmms.Application.WorkOrders.Commands.AssignTechnician;
using CleanArchitecture.Cmms.Domain.Technicians;
using CleanArchitecture.Cmms.Domain.Technicians.ValueObjects;
using CleanArchitecture.Cmms.Domain.WorkOrders;
using CleanArchitecture.Cmms.Domain.WorkOrders.ValueObjects;

namespace CleanArchitecture.Cmms.Application.UnitTests.WorkOrders.Commands.AssignTechnician;

public class AssignTechnicianCommandHandlerTests
{
    private readonly Mock<IRepository<WorkOrder, Guid>> _workOrderRepositoryMock;
    private readonly Mock<IRepository<Technician, Guid>> _technicianRepositoryMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly AssignTechnicianCommandHandler _sut;

    public AssignTechnicianCommandHandlerTests()
    {
        _workOrderRepositoryMock = new Mock<IRepository<WorkOrder, Guid>>();
        _technicianRepositoryMock = new Mock<IRepository<Technician, Guid>>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        _sut = new AssignTechnicianCommandHandler(
            _workOrderRepositoryMock.Object,
            _technicianRepositoryMock.Object,
            _dateTimeProviderMock.Object);
    }

    [Fact]
    public async Task Handle_WhenTechnicianAndWorkOrderExist_ShouldAssignTechnician()
    {
        // Arrange
        var technician = CreateTechnician();
        var workOrder = CreateWorkOrder();
        var utcNow = DateTime.UtcNow;
        var command = new AssignTechnicianCommand(workOrder.Id, technician.Id);

        _technicianRepositoryMock.Setup(x => x.GetByIdAsync(technician.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(technician);

        _workOrderRepositoryMock.Setup(x => x.GetByIdAsync(workOrder.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workOrder);

        _dateTimeProviderMock.Setup(x => x.UtcNow).Returns(utcNow);

        _technicianRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Technician>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _workOrderRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<WorkOrder>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _technicianRepositoryMock.Verify(x => x.GetByIdAsync(technician.Id, It.IsAny<CancellationToken>()), Times.Once);
        _workOrderRepositoryMock.Verify(x => x.GetByIdAsync(workOrder.Id, It.IsAny<CancellationToken>()), Times.Once);
        _technicianRepositoryMock.Verify(x => x.UpdateAsync(technician, It.IsAny<CancellationToken>()), Times.Once);
        _workOrderRepositoryMock.Verify(x => x.UpdateAsync(workOrder, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTechnicianNotFound_ShouldReturnFailure()
    {
        // Arrange
        var technicianId = Guid.NewGuid();
        var workOrder = CreateWorkOrder();
        var command = new AssignTechnicianCommand(workOrder.Id, technicianId);

        _technicianRepositoryMock.Setup(x => x.GetByIdAsync(technicianId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Technician?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Be("Technician not found.");
        result.Error.Type.Should().Be(ErrorType.NotFound);

        _technicianRepositoryMock.Verify(x => x.GetByIdAsync(technicianId, It.IsAny<CancellationToken>()), Times.Once);
        _workOrderRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenWorkOrderNotFound_ShouldReturnFailure()
    {
        // Arrange
        var technician = CreateTechnician();
        var workOrderId = Guid.NewGuid();
        var command = new AssignTechnicianCommand(workOrderId, technician.Id);

        _technicianRepositoryMock.Setup(x => x.GetByIdAsync(technician.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(technician);

        _workOrderRepositoryMock.Setup(x => x.GetByIdAsync(workOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkOrder?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Be("Work order not found.");
        result.Error.Type.Should().Be(ErrorType.NotFound);

        _technicianRepositoryMock.Verify(x => x.GetByIdAsync(technician.Id, It.IsAny<CancellationToken>()), Times.Once);
        _workOrderRepositoryMock.Verify(x => x.GetByIdAsync(workOrderId, It.IsAny<CancellationToken>()), Times.Once);
    }

    private static Technician CreateTechnician()
    {
        var skillLevel = SkillLevel.Master;
        var technician = Technician.Create("John Doe", skillLevel);
        return technician;
    }

    private static WorkOrder CreateWorkOrder()
    {
        var location = Location.Create("Building A", "Floor 1", "Room 101");
        var workOrder = WorkOrder.Create(Guid.NewGuid(), "Test Work Order", location);
        return workOrder;
    }
}
