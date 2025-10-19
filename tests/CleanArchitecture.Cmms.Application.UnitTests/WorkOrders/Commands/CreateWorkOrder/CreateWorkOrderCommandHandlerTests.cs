using CleanArchitecture.Cmms.Application.Abstractions.Persistence.Repositories;
using CleanArchitecture.Cmms.Application.WorkOrders.Commands.CreateWorkOrder;
using CleanArchitecture.Cmms.Domain.Assets;
using CleanArchitecture.Cmms.Domain.Assets.ValueObjects;
using CleanArchitecture.Cmms.Domain.WorkOrders;

namespace CleanArchitecture.Cmms.Application.UnitTests.WorkOrders.Commands.CreateWorkOrder;

public class CreateWorkOrderCommandHandlerTests
{
    private readonly Mock<IRepository<WorkOrder, Guid>> _workOrderRepositoryMock;
    private readonly Mock<IRepository<Asset, Guid>> _assetRepositoryMock;
    private readonly CreateWorkOrderCommandHandler _sut;

    public CreateWorkOrderCommandHandlerTests()
    {
        _workOrderRepositoryMock = new Mock<IRepository<WorkOrder, Guid>>();
        _assetRepositoryMock = new Mock<IRepository<Asset, Guid>>();
        _sut = new CreateWorkOrderCommandHandler(_workOrderRepositoryMock.Object, _assetRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenAssetExistsAndIsAvailable_ShouldCreateWorkOrder()
    {
        // Arrange
        var asset = CreateAvailableAsset();
        var command = new CreateWorkOrderCommand(asset.Id, "Test Work Order", "Building A", "Floor 1", "Room 101");

        _assetRepositoryMock.Setup(x => x.GetByIdAsync(asset.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(asset);

        _workOrderRepositoryMock.Setup(x => x.AddAsync(It.IsAny<WorkOrder>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        _assetRepositoryMock.Verify(x => x.GetByIdAsync(asset.Id, It.IsAny<CancellationToken>()), Times.Once);
        _workOrderRepositoryMock.Verify(x => x.AddAsync(It.IsAny<WorkOrder>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAssetNotFound_ShouldReturnFailure()
    {
        // Arrange
        var assetId = Guid.NewGuid();
        var command = new CreateWorkOrderCommand(assetId, "Test Work Order", "Building A", "Floor 1", "Room 101");

        _assetRepositoryMock.Setup(x => x.GetByIdAsync(assetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Asset?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(Application.Assets.AssetErrors.NotFound.Code);
        result.Error.Message.Should().Be(Application.Assets.AssetErrors.NotFound.Message);

        _assetRepositoryMock.Verify(x => x.GetByIdAsync(assetId, It.IsAny<CancellationToken>()), Times.Once);
        _workOrderRepositoryMock.Verify(x => x.AddAsync(It.IsAny<WorkOrder>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenAssetIsNotAvailable_ShouldReturnFailure()
    {
        // Arrange
        var asset = CreateUnavailableAsset();
        var command = new CreateWorkOrderCommand(asset.Id, "Test Work Order", "Building A", "Floor 1", "Room 101");

        _assetRepositoryMock.Setup(x => x.GetByIdAsync(asset.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(asset);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(Application.Assets.AssetErrors.NotAvailable.Code);
        result.Error.Message.Should().Be(Application.Assets.AssetErrors.NotAvailable.Message);

        _assetRepositoryMock.Verify(x => x.GetByIdAsync(asset.Id, It.IsAny<CancellationToken>()), Times.Once);
        _workOrderRepositoryMock.Verify(x => x.AddAsync(It.IsAny<WorkOrder>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static Asset CreateAvailableAsset()
    {
        var tag = AssetTag.Create("TAG001");
        var location = AssetLocation.Create("Main Site", "Production", "Zone A");
        var asset = Asset.Create("Test Asset", "Equipment", tag, location);
        return asset;
    }

    private static Asset CreateUnavailableAsset()
    {
        var tag = AssetTag.Create("TAG002");
        var location = AssetLocation.Create("Main Site", "Production", "Zone B");

        var asset = Asset.Create("Test Asset", "Equipment", tag, location);

        asset.SetUnderMaintenance("Routine Checkup", "Test", DateTime.UtcNow);

        return asset;
    }
}
