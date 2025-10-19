using CleanArchitecture.Cmms.Application.Abstractions.Persistence;
using CleanArchitecture.Cmms.Application.Abstractions.Persistence.Repositories;
using CleanArchitecture.Cmms.Application.Primitives;
using CleanArchitecture.Cmms.Application.Technicians.Queries.GetTechnicianAssignments;
using CleanArchitecture.Cmms.Domain.Technicians;
using CleanArchitecture.Cmms.Domain.Technicians.ValueObjects;

namespace CleanArchitecture.Cmms.Application.UnitTests.Technicians.Queries.GetAvailableTechnicians;

public class GetTechnicianAssignmentsQueryHandlerTests
{
    private readonly Mock<IReadRepository<Technician, Guid>> _repositoryMock;
    private readonly GetTechnicianAssignmentsQueryHandler _sut;

    public GetTechnicianAssignmentsQueryHandlerTests()
    {
        _repositoryMock = new Mock<IReadRepository<Technician, Guid>>();
        _sut = new GetTechnicianAssignmentsQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenTechnicianExists_ShouldReturnAssignments()
    {
        // Arrange
        var technician = CreateTechnicianWithAssignments();
        var pagination = new PaginationParam(1, 10);
        var query = new GetTechnicianAssignmentsQuery(technician.Id, pagination);
        var paginatedTechnicians = PaginatedList<Technician>.Create(new List<Technician> { technician }, 1, 1, 10);

        _repositoryMock.Setup(x => x.ListAsync(It.IsAny<Criteria<Technician>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paginatedTechnicians);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.TotalCount.Should().Be(1);

        var assignment = result.Value.Items[0];
        assignment.WorkOrderId.Should().NotBeEmpty();
        assignment.AssignedOn.Should().NotBe(default);
        assignment.CompletedOn.Should().BeNull();

        _repositoryMock.Verify(x => x.ListAsync(It.IsAny<Criteria<Technician>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTechnicianNotFound_ShouldReturnFailure()
    {
        // Arrange
        var technicianId = Guid.NewGuid();
        var pagination = new PaginationParam(1, 10);
        var query = new GetTechnicianAssignmentsQuery(technicianId, pagination);
        var emptyTechnicians = PaginatedList<Technician>.Create(new List<Technician>(), 0, 1, 10);

        _repositoryMock.Setup(x => x.ListAsync(It.IsAny<Criteria<Technician>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyTechnicians);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(Application.Technicians.TechnicianErrors.NotFound.Code);
        result.Error.Message.Should().Be(Application.Technicians.TechnicianErrors.NotFound.Message);

        _repositoryMock.Verify(x => x.ListAsync(It.IsAny<Criteria<Technician>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTechnicianHasNoAssignments_ShouldReturnEmptyList()
    {
        // Arrange
        var technician = CreateTechnicianWithoutAssignments();
        var pagination = new PaginationParam(1, 10);
        var query = new GetTechnicianAssignmentsQuery(technician.Id, pagination);
        var paginatedTechnicians = PaginatedList<Technician>.Create(new List<Technician> { technician }, 1, 1, 10);

        _repositoryMock.Setup(x => x.ListAsync(It.IsAny<Criteria<Technician>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paginatedTechnicians);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(1);

        _repositoryMock.Verify(x => x.ListAsync(It.IsAny<Criteria<Technician>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private static Technician CreateTechnicianWithAssignments()
    {
        var skillLevel = SkillLevel.Create("Senior", 5);
        var technician = Technician.Create("John Doe", skillLevel);

        // Add an assignment
        technician.AddAssignedOrder(Guid.NewGuid(), DateTime.UtcNow);

        return technician;
    }

    private static Technician CreateTechnicianWithoutAssignments()
    {
        var skillLevel = SkillLevel.Create("Junior", 3);
        var technician = Technician.Create("Jane Smith", skillLevel);
        return technician;
    }
}
