using CleanArchitecture.Cmms.Application.Abstractions.Persistence.Repositories;
using CleanArchitecture.Cmms.Application.Technicians.Commands.CompleteAssignment;
using CleanArchitecture.Cmms.Domain.Technicians;
using CleanArchitecture.Cmms.Domain.Technicians.ValueObjects;

namespace CleanArchitecture.Cmms.Application.UnitTests.Technicians.Commands.CompleteAssignment;

public class CompleteAssignmentCommandHandlerTests
{
    private readonly Mock<IRepository<Technician, Guid>> _repositoryMock;
    private readonly CompleteAssignmentCommandHandler _sut;

    public CompleteAssignmentCommandHandlerTests()
    {
        _repositoryMock = new Mock<IRepository<Technician, Guid>>();
        _sut = new CompleteAssignmentCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenTechnicianExists_ShouldCompleteAssignment()
    {
        // Arrange
        var workOrderId = Guid.NewGuid();
        var technician = CreateTechnician(workOrderId);
        var completedOn = DateTime.UtcNow;
        var command = new CompleteAssignmentCommand(technician.Id, workOrderId, completedOn);

        _repositoryMock.Setup(x => x.GetByIdAsync(technician.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(technician);

        _repositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Technician>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _repositoryMock.Verify(x => x.GetByIdAsync(technician.Id, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.UpdateAsync(technician, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTechnicianNotFound_ShouldReturnFailure()
    {
        // Arrange
        var technicianId = Guid.NewGuid();
        var workOrderId = Guid.NewGuid();
        var completedOn = DateTime.UtcNow;
        var command = new CompleteAssignmentCommand(technicianId, workOrderId, completedOn);

        _repositoryMock.Setup(x => x.GetByIdAsync(technicianId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Technician?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(Application.Technicians.TechnicianErrors.NotFound.Code);
        result.Error.Message.Should().Be(Application.Technicians.TechnicianErrors.NotFound.Message);

        _repositoryMock.Verify(x => x.GetByIdAsync(technicianId, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Technician>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static Technician CreateTechnician(Guid orderId)
    {
        var skillLevel = SkillLevel.Journeyman;
        var technician = Technician.Create("John Doe", skillLevel);

        technician.AddAssignedOrder(orderId, DateTime.UtcNow.AddDays(-1));
        return technician;
    }
}
