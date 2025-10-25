using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;
using CleanArchitecture.Cmms.Application.Assets.Commands.CreateAsset;
using CleanArchitecture.Cmms.Domain.Assets;

namespace CleanArchitecture.Cmms.Application.UnitTests.Assets.Commands.CreateAsset;

public class CreateAssetCommandHandlerTests
{
    private readonly Mock<IRepository<Asset, Guid>> _repositoryMock;
    private readonly CreateAssetCommandHandler _sut;

    public CreateAssetCommandHandlerTests()
    {
        _repositoryMock = new Mock<IRepository<Asset, Guid>>();
        _sut = new CreateAssetCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateAsset()
    {
        // Arrange
        var command = new CreateAssetCommand(
            Name: "Test Asset",
            Type: "Equipment",
            TagCode: "TAG001",
            Site: "Main Site",
            Area: "Production",
            Zone: "Zone A"
        );

        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<Asset>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Asset>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
