using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Application.Abstractions.Query;
using CleanArchitecture.Cmms.Application.WorkOrders.Dtos;
using CleanArchitecture.Cmms.Application.WorkOrders.Interfaces;
using CleanArchitecture.Cmms.Application.WorkOrders.Queries.GetActiveWorkOrder;

namespace CleanArchitecture.Cmms.Application.UnitTests.WorkOrders.Queries.GetActiveWorkOrders;

public class GetActiveWorkOrdersQueryHandlerTests
{
    private readonly Mock<IWorkOrderReadRepository> _repositoryMock;
    private readonly GetActiveWorkOrdersQueryHandler _sut;

    public GetActiveWorkOrdersQueryHandlerTests()
    {
        _repositoryMock = new Mock<IWorkOrderReadRepository>();
        _sut = new GetActiveWorkOrdersQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryWithPagination()
    {
        // Arrange
        var pagination = new PaginationParam(1, 10);
        var query = new GetActiveWorkOrdersQuery(pagination);
        var repoResult = PaginatedList<WorkOrderListItemDto>.Create(new List<WorkOrderListItemDto>(), 0, 1, 10);

        var expectedResult = Result<PaginatedList<WorkOrderListItemDto>>.Success(repoResult);

        _repositoryMock.Setup(x => x.GetActiveWithTechnicianAndAssetAsync(pagination, It.IsAny<CancellationToken>()))
            .ReturnsAsync(repoResult);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);

        _repositoryMock.Verify(x => x.GetActiveWithTechnicianAndAssetAsync(pagination, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDifferentPagination_ShouldPassCorrectParameters()
    {
        // Arrange
        var pagination = new PaginationParam(2, 5);
        var query = new GetActiveWorkOrdersQuery(pagination);
        var repoResult = PaginatedList<WorkOrderListItemDto>.Create(new List<WorkOrderListItemDto>(), 0, 2, 5);
        var expectedResult = Result<PaginatedList<WorkOrderListItemDto>>.Success(repoResult);

        _repositoryMock.Setup(x => x.GetActiveWithTechnicianAndAssetAsync(pagination, It.IsAny<CancellationToken>()))
            .ReturnsAsync(repoResult);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);

        _repositoryMock.Verify(x => x.GetActiveWithTechnicianAndAssetAsync(pagination, It.IsAny<CancellationToken>()), Times.Once);
    }
}
