using CleanArchitecture.Cmms.Application.Technicians.Commands.SetUnavailable;
using CleanArchitecture.Cmms.Domain.Technicians;
using CleanArchitecture.Cmms.Domain.Technicians.Enums;
using CleanArchitecture.Cmms.Domain.Technicians.ValueObjects;
using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;

namespace CleanArchitecture.Cmms.Application.UnitTests.Technicians.Commands.SetUnavailable;

public class SetUnavailableCommandHandlerTests
{
    private readonly Mock<IRepository<Technician, Guid>> _repositoryMock;
    private readonly SetUnavailableCommandHandler _handler;

    public SetUnavailableCommandHandlerTests()
    {
        _repositoryMock = new Mock<IRepository<Technician, Guid>>();
        _handler = new SetUnavailableCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldSetUnavailable()
    {
        // Arrange
        var technicianId = Guid.NewGuid();
        var technician = Technician.Create("John Doe", SkillLevel.Journeyman);
        var command = new SetUnavailableCommand(TechnicianId: technicianId);

        _repositoryMock.Setup(x => x.GetByIdAsync(technicianId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(technician);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        technician.Status.Should().Be(TechnicianStatus.Unavailable);
    }

    [Fact]
    public async Task Handle_WithTechnicianNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        var technicianId = Guid.NewGuid();
        var command = new SetUnavailableCommand(TechnicianId: technicianId);

        _repositoryMock.Setup(x => x.GetByIdAsync(technicianId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Technician?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(Application.Technicians.TechnicianErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WhenAlreadyUnavailable_ShouldBeIdempotent()
    {
        // Arrange
        var technicianId = Guid.NewGuid();
        var technician = Technician.Create("John Doe", SkillLevel.Journeyman);
        technician.SetUnavailable(); // Already unavailable
        var command = new SetUnavailableCommand(TechnicianId: technicianId);

        _repositoryMock.Setup(x => x.GetByIdAsync(technicianId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(technician);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        technician.Status.Should().Be(TechnicianStatus.Unavailable);
    }
}
