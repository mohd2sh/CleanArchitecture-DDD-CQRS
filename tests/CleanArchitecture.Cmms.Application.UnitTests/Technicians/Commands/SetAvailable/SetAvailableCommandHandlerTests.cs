using CleanArchitecture.Cmms.Application.Technicians.Commands.SetAvailable;
using CleanArchitecture.Cmms.Domain.Technicians;
using CleanArchitecture.Cmms.Domain.Technicians.Enums;
using CleanArchitecture.Cmms.Domain.Technicians.ValueObjects;
using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;

namespace CleanArchitecture.Cmms.Application.UnitTests.Technicians.Commands.SetAvailable;

public class SetAvailableCommandHandlerTests
{
    private readonly Mock<IRepository<Technician, Guid>> _repositoryMock;
    private readonly SetAvailableCommandHandler _handler;

    public SetAvailableCommandHandlerTests()
    {
        _repositoryMock = new Mock<IRepository<Technician, Guid>>();
        _handler = new SetAvailableCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldSetAvailable()
    {
        // Arrange
        var technicianId = Guid.NewGuid();
        var technician = Technician.Create("John Doe", SkillLevel.Journeyman);
        technician.SetUnavailable(); // Start as unavailable
        var command = new SetAvailableCommand(TechnicianId: technicianId);

        _repositoryMock.Setup(x => x.GetByIdAsync(technicianId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(technician);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        technician.Status.Should().Be(TechnicianStatus.Available);
    }

    [Fact]
    public async Task Handle_WithTechnicianNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        var technicianId = Guid.NewGuid();
        var command = new SetAvailableCommand(TechnicianId: technicianId);

        _repositoryMock.Setup(x => x.GetByIdAsync(technicianId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Technician?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(Application.Technicians.TechnicianErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WhenAlreadyAvailable_ShouldBeIdempotent()
    {
        // Arrange
        var technicianId = Guid.NewGuid();
        var technician = Technician.Create("John Doe", SkillLevel.Journeyman);
        // Already available by default
        var command = new SetAvailableCommand(TechnicianId: technicianId);

        _repositoryMock.Setup(x => x.GetByIdAsync(technicianId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(technician);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        technician.Status.Should().Be(TechnicianStatus.Available);
    }
}
