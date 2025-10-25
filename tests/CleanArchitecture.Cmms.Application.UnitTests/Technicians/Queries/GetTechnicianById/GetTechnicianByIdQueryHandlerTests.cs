using CleanArchitecture.Cmms.Application.Technicians.Queries.GetTechnicianById;
using CleanArchitecture.Cmms.Domain.Technicians;
using CleanArchitecture.Cmms.Domain.Technicians.ValueObjects;
using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;

namespace CleanArchitecture.Cmms.Application.UnitTests.Technicians.Queries.GetTechnicianById;

public class GetTechnicianByIdQueryHandlerTests
{
    private readonly Mock<IReadRepository<Technician, Guid>> _repositoryMock;
    private readonly GetTechnicianByIdQueryHandler _sut;

    public GetTechnicianByIdQueryHandlerTests()
    {
        _repositoryMock = new Mock<IReadRepository<Technician, Guid>>();
        _sut = new GetTechnicianByIdQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenTechnicianExists_ShouldReturnTechnicianDto()
    {
        // Arrange
        var technician = CreateTechnician();
        var query = new GetTechnicianByIdQuery(technician.Id);

        _repositoryMock.Setup(x => x.GetByIdAsync(technician.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(technician);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(technician.Id);
        result.Value.Name.Should().Be("John Doe");
        result.Value.SkillLevelName.Should().Be("Master");
        result.Value.SkillLevelRank.Should().Be(3);
        result.Value.Status.Should().Be("Available");
        result.Value.ActiveAssignmentsCount.Should().Be(0);
        result.Value.TotalCertifications.Should().Be(0);

        _repositoryMock.Verify(x => x.GetByIdAsync(technician.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTechnicianNotFound_ShouldReturnFailure()
    {
        // Arrange
        var technicianId = Guid.NewGuid();
        var query = new GetTechnicianByIdQuery(technicianId);

        _repositoryMock.Setup(x => x.GetByIdAsync(technicianId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Technician?)null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(Application.Technicians.TechnicianErrors.NotFound.Code);
        result.Error.Message.Should().Be(Application.Technicians.TechnicianErrors.NotFound.Message);

        _repositoryMock.Verify(x => x.GetByIdAsync(technicianId, It.IsAny<CancellationToken>()), Times.Once);
    }

    private static Technician CreateTechnician()
    {
        var skillLevel = SkillLevel.Master;
        var technician = Technician.Create("John Doe", skillLevel);
        return technician;
    }
}
