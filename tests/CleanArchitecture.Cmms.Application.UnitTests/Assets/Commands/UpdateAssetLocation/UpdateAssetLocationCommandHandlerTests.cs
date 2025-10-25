using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;
using CleanArchitecture.Cmms.Application.Assets.Commands.UpdateAssetLocation;
using CleanArchitecture.Cmms.Domain.Assets;
using CleanArchitecture.Cmms.Domain.Assets.ValueObjects;

namespace CleanArchitecture.Cmms.Application.UnitTests.Assets.Commands.UpdateAssetLocation;

public class UpdateAssetLocationCommandHandlerTests
{
    private readonly Mock<IRepository<Asset, Guid>> _repositoryMock;
    private readonly UpdateAssetLocationCommandHandler _sut;

    public UpdateAssetLocationCommandHandlerTests()
    {
        _repositoryMock = new Mock<IRepository<Asset, Guid>>();
        _sut = new UpdateAssetLocationCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenAssetExists_ShouldUpdateLocation()
    {
        // Arrange
        var asset = CreateAsset();
        var command = new UpdateAssetLocationCommand(asset.Id, "New Site", "New Area", "New Zone");

        _repositoryMock.Setup(x => x.GetByIdAsync(asset.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(asset);

        _repositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Asset>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _repositoryMock.Verify(x => x.GetByIdAsync(asset.Id, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.UpdateAsync(asset, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAssetNotFound_ShouldReturnFailure()
    {
        // Arrange
        var assetId = Guid.NewGuid();
        var command = new UpdateAssetLocationCommand(assetId, "New Site", "New Area", "New Zone");

        _repositoryMock.Setup(x => x.GetByIdAsync(assetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Asset?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(Application.Assets.AssetErrors.NotFound.Code);
        result.Error.Message.Should().Be(Application.Assets.AssetErrors.NotFound.Message);

        _repositoryMock.Verify(x => x.GetByIdAsync(assetId, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Asset>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static Asset CreateAsset()
    {
        var tag = AssetTag.Create("TAG001");
        var location = AssetLocation.Create("Main Site", "Production", "Zone A");
        var asset = Asset.Create("Test Asset", "Equipment", tag, location);
        return asset;
    }
}
