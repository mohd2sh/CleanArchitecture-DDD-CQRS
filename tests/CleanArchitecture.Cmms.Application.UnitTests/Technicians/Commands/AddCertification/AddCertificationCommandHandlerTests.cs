using CleanArchitecture.Cmms.Application.Technicians.Commands.AddCertification;
using CleanArchitecture.Cmms.Domain.Technicians;
using CleanArchitecture.Cmms.Domain.Technicians.ValueObjects;
using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;
using CleanArchitecture.Core.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Application.UnitTests.Technicians.Commands.AddCertification;

public class AddCertificationCommandHandlerTests
{
    private readonly Mock<IRepository<Technician, Guid>> _repositoryMock;
    private readonly AddCertificationCommandHandler _handler;

    public AddCertificationCommandHandlerTests()
    {
        _repositoryMock = new Mock<IRepository<Technician, Guid>>();
        _handler = new AddCertificationCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldAddCertification()
    {
        // Arrange
        var technicianId = Guid.NewGuid();
        var technician = Technician.Create("John Doe", SkillLevel.Journeyman);
        var command = new AddCertificationCommand(
            TechnicianId: technicianId,
            Code: "HVAC-001",
            IssuedOn: DateTime.UtcNow.Date,
            ExpiresOn: DateTime.UtcNow.Date.AddYears(1)
        );

        _repositoryMock.Setup(x => x.GetByIdAsync(technicianId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(technician);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        technician.Certifications.Should().HaveCount(1);
        technician.Certifications.First().Code.Should().Be("HVAC-001");
    }

    [Fact]
    public async Task Handle_WithTechnicianNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        var technicianId = Guid.NewGuid();
        var command = new AddCertificationCommand(
            TechnicianId: technicianId,
            Code: "HVAC-001",
            IssuedOn: DateTime.UtcNow.Date,
            ExpiresOn: DateTime.UtcNow.Date.AddYears(1)
        );

        _repositoryMock.Setup(x => x.GetByIdAsync(technicianId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Technician?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(Application.Technicians.TechnicianErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WithDuplicateCertification_ShouldThrowDomainException()
    {
        // Arrange
        var technicianId = Guid.NewGuid();
        var technician = Technician.Create("John Doe", SkillLevel.Journeyman);
        var existingCertification = Certification.Create("HVAC-001", DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddYears(1));
        technician.AddCertification(existingCertification);

        var command = new AddCertificationCommand(
            TechnicianId: technicianId,
            Code: "HVAC-001",
            IssuedOn: DateTime.UtcNow.Date,
            ExpiresOn: DateTime.UtcNow.Date.AddYears(1)
        );

        _repositoryMock.Setup(x => x.GetByIdAsync(technicianId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(technician);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>();
    }
}
