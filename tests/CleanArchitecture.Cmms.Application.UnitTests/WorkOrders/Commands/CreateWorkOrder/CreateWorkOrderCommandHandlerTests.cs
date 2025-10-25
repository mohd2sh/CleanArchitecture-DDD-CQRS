using CleanArchitecture.Cmms.Application.WorkOrders.Commands.CreateWorkOrder;
using CleanArchitecture.Cmms.Domain.WorkOrders;
using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;

namespace CleanArchitecture.Cmms.Application.UnitTests.WorkOrders.Commands.CreateWorkOrder;

public class CreateWorkOrderCommandHandlerTests
{
    private readonly Mock<IRepository<WorkOrder, Guid>> _workOrderRepositoryMock;
    private readonly CreateWorkOrderCommandHandler _sut;

    public CreateWorkOrderCommandHandlerTests()
    {
        _workOrderRepositoryMock = new Mock<IRepository<WorkOrder, Guid>>();
        _sut = new CreateWorkOrderCommandHandler(_workOrderRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenValidCommand_ShouldCreateWorkOrder()
    {
        // Arrange
        var assetId = Guid.NewGuid();
        var command = new CreateWorkOrderCommand(
            AssetId: assetId,
            Title: "Fix Machine",
            Building: "Building A",
            Floor: "Floor 1",
            Room: "Room 101");

        _workOrderRepositoryMock.Setup(x => x.AddAsync(It.IsAny<WorkOrder>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);

        _workOrderRepositoryMock.Verify(x => x.AddAsync(It.IsAny<WorkOrder>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenValidCommand_ShouldCreateWorkOrderWithCorrectProperties()
    {
        // Arrange
        var assetId = Guid.NewGuid();
        var command = new CreateWorkOrderCommand(
            AssetId: assetId,
            Title: "Fix Machine",
            Building: "Building A",
            Floor: "Floor 1",
            Room: "Room 101");

        WorkOrder? capturedWorkOrder = null;
        _workOrderRepositoryMock.Setup(x => x.AddAsync(It.IsAny<WorkOrder>(), It.IsAny<CancellationToken>()))
            .Callback<WorkOrder, CancellationToken>((wo, ct) => capturedWorkOrder = wo)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedWorkOrder.Should().NotBeNull();
        capturedWorkOrder!.AssetId.Should().Be(assetId);
        capturedWorkOrder.Title.Should().Be("Fix Machine");
        capturedWorkOrder.Location.Building.Should().Be("Building A");
        capturedWorkOrder.Location.Floor.Should().Be("Floor 1");
        capturedWorkOrder.Location.Room.Should().Be("Room 101");
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
    {
        // Arrange
        var assetId = Guid.NewGuid();
        var command = new CreateWorkOrderCommand(
            AssetId: assetId,
            Title: "Fix Machine",
            Building: "Building A",
            Floor: "Floor 1",
            Room: "Room 101");

        var expectedException = new InvalidOperationException("Database error");
        _workOrderRepositoryMock.Setup(x => x.AddAsync(It.IsAny<WorkOrder>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.Handle(command, CancellationToken.None));

        exception.Should().Be(expectedException);
    }
}
