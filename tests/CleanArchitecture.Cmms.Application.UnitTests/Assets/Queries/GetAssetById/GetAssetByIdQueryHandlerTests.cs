using CleanArchitecture.Cmms.Application.Assets.Queries.GetAssetById;
using CleanArchitecture.Cmms.Domain.Assets;
using CleanArchitecture.Cmms.Domain.Assets.ValueObjects;
using CleanArchitecture.Core.Application.Abstractions.Persistence;
using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;

namespace CleanArchitecture.Cmms.Application.UnitTests.Assets.Queries.GetAssetById;

public class GetAssetByIdQueryHandlerTests
{
    private readonly Mock<IReadRepository<Asset, Guid>> _repositoryMock;
    private readonly GetAssetByIdQueryHandler _sut;

    public GetAssetByIdQueryHandlerTests()
    {
        _repositoryMock = new Mock<IReadRepository<Asset, Guid>>();
        _sut = new GetAssetByIdQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenAssetExists_ShouldReturnAssetDto()
    {
        // Arrange
        var asset = CreateMockAsset();
        var assetId = asset.Id;
        var query = new GetAssetByIdQuery(assetId);

        _repositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Criteria<Asset>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(asset);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().NotBe(Guid.Empty);
        result.Value.Name.Should().Be("Temp");
        result.Value.TagValue.Should().Be("Tag 1");
        result.Value.TotalMaintenanceRecords.Should().Be(0);

        _repositoryMock.Verify(x => x.FirstOrDefaultAsync(It.IsAny<Criteria<Asset>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAssetNotFound_ShouldReturnFailure()
    {
        // Arrange
        var assetId = Guid.NewGuid();
        var query = new GetAssetByIdQuery(assetId);

        _repositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Criteria<Asset>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Asset?)null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(Application.Assets.AssetErrors.NotFound.Code);
        result.Error.Message.Should().Be(Application.Assets.AssetErrors.NotFound.Message);

        _repositoryMock.Verify(x => x.FirstOrDefaultAsync(It.IsAny<Criteria<Asset>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private static Asset CreateMockAsset()
    {
        var tag = AssetTag.Create("Tag 1");
        var location = AssetLocation.Create("Main Site", "Production", "Zone A");
        var asset = Asset.Create("Temp", "TempType", tag, location);

        return asset;
    }
}
