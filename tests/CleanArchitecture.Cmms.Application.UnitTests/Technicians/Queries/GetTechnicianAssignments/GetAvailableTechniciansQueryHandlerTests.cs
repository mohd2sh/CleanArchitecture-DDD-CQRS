using CleanArchitecture.Core.Application.Abstractions.Persistence;
using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;
using CleanArchitecture.Core.Application.Abstractions.Query;
using CleanArchitecture.Cmms.Application.Technicians.Queries.GetAvailableTechnicians;
using CleanArchitecture.Cmms.Domain.Technicians;
using CleanArchitecture.Cmms.Domain.Technicians.ValueObjects;

namespace CleanArchitecture.Cmms.Application.UnitTests.Technicians.Queries.GetTechnicianAssignments;

public class GetAvailableTechniciansQueryHandlerTests
{
    private readonly Mock<IReadRepository<Technician, Guid>> _repositoryMock;
    private readonly GetAvailableTechniciansQueryHandler _sut;

    public GetAvailableTechniciansQueryHandlerTests()
    {
        _repositoryMock = new Mock<IReadRepository<Technician, Guid>>();
        _sut = new GetAvailableTechniciansQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAvailableTechnicians()
    {
        // Arrange
        var pagination = new PaginationParam(1, 10);
        var query = new GetAvailableTechniciansQuery(pagination);
        var technicians = CreateAvailableTechnicians();
        var paginatedTechnicians = PaginatedList<Technician>.Create(technicians, 2, 1, 10);

        _repositoryMock.Setup(x => x.ListAsync(It.IsAny<Criteria<Technician>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paginatedTechnicians);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        Assert.NotNull(result.Value);
        result.Value.Items.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(2);
        result.Value.PageNumber.Should().Be(1);
        result.Value.PageSize.Should().Be(10);

        var firstTechnician = result.Value.Items[0];
        firstTechnician.Id.Should().NotBeEmpty();
        firstTechnician.Name.Should().Be("John Doe");
        firstTechnician.SkillLevelName.Should().Be("Senior");
        firstTechnician.ActiveAssignmentsCount.Should().Be(0);
        firstTechnician.TotalCertifications.Should().Be(0);

        _repositoryMock.Verify(x => x.ListAsync(It.IsAny<Criteria<Technician>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDifferentPagination_ShouldUseCorrectParameters()
    {
        // Arrange
        var pagination = new PaginationParam(2, 5);
        var query = new GetAvailableTechniciansQuery(pagination);
        var technicians = CreateAvailableTechnicians();
        var paginatedTechnicians = PaginatedList<Technician>.Create(technicians, 2, 2, 5);

        _repositoryMock.Setup(x => x.ListAsync(It.IsAny<Criteria<Technician>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paginatedTechnicians);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        Assert.NotNull(result.Value);
        result.Value.Items.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(2);
        result.Value.PageNumber.Should().Be(2);
        result.Value.PageSize.Should().Be(5);

        _repositoryMock.Verify(x => x.ListAsync(It.IsAny<Criteria<Technician>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNoTechniciansFound_ShouldReturnEmptyList()
    {
        // Arrange
        var pagination = new PaginationParam(1, 10);
        var query = new GetAvailableTechniciansQuery(pagination);
        var emptyTechnicians = new List<Technician>();
        var paginatedTechnicians = PaginatedList<Technician>.Create(emptyTechnicians, 0, 1, 10);

        _repositoryMock.Setup(x => x.ListAsync(It.IsAny<Criteria<Technician>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paginatedTechnicians);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        Assert.NotNull(result.Value);
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
        result.Value.PageNumber.Should().Be(1);
        result.Value.PageSize.Should().Be(10);

        _repositoryMock.Verify(x => x.ListAsync(It.IsAny<Criteria<Technician>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private static List<Technician> CreateAvailableTechnicians()
    {
        var skillLevel1 = SkillLevel.Create("Senior", 5);
        var technician1 = Technician.Create("John Doe", skillLevel1);

        var skillLevel2 = SkillLevel.Create("Junior", 3);
        var technician2 = Technician.Create("Jane Smith", skillLevel2);

        return new List<Technician> { technician1, technician2 };
    }
}
