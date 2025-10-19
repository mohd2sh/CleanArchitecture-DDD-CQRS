using CleanArchitecture.Cmms.Application.Abstractions.Persistence.Repositories;
using CleanArchitecture.Cmms.Application.Technicians.Commands.CreateTechnician;
using CleanArchitecture.Cmms.Domain.Technicians;

namespace CleanArchitecture.Cmms.Application.UnitTests.Technicians.Commands.CreateTechnician;

public class CreateTechnicianCommandHandlerTests
{
    private readonly Mock<IRepository<Technician, Guid>> _repositoryMock;
    private readonly CreateTechnicianCommandHandler _handler;

    public CreateTechnicianCommandHandlerTests()
    {
        _repositoryMock = new Mock<IRepository<Technician, Guid>>();
        _handler = new CreateTechnicianCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateTechnician()
    {
        // Arrange
        var command = new CreateTechnicianCommand(
            Name: "John Doe",
            SkillLevelName: "Senior",
            SkillLevelRank: 5
        );

        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<Technician>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Technician>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
