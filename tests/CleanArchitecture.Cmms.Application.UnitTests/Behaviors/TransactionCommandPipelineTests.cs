using CleanArchitecture.Core.Application.Abstractions.Messaging;
using CleanArchitecture.Core.Application.Abstractions.Persistence;
using CleanArchitecture.Cmms.Application.Behaviors;
using Microsoft.Extensions.Logging.Abstractions;

namespace CleanArchitecture.Cmms.Application.UnitTests.Behaviors;

public class TransactionCommandPipelineTests
{
    public sealed record DummyCommand : ICommand<int>;

    [Fact]
    public async Task Should_Begin_Save_Commit_On_Success()
    {
        // Arrange
        var expectedValue = 7;
        var uow = new Mock<IUnitOfWork>();
        var tx = new Mock<ITransaction>();
        uow.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tx.Object);
        var logger = NullLogger<TransactionCommandPipeline<DummyCommand, int>>.Instance;

        var pipe = new TransactionCommandPipeline<DummyCommand, int>(uow.Object, logger);

        // Act
        var result = await pipe.Handle(new DummyCommand(), async () => { await Task.Yield(); return expectedValue; }, CancellationToken.None);

        // Assert
        Assert.Equal(expectedValue, result);
        uow.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        tx.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        tx.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Should_Rollback_On_Exception_And_Rethrow()
    {
        // Arrange
        var uow = new Mock<IUnitOfWork>();
        var tx = new Mock<ITransaction>();
        uow.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tx.Object);
        var logger = NullLogger<TransactionCommandPipeline<DummyCommand, int>>.Instance;
        var pipe = new TransactionCommandPipeline<DummyCommand, int>(uow.Object, logger);

        // Act
        var act = async () => await pipe.Handle(new DummyCommand(), () => throw new InvalidOperationException("boom"), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(act);
        tx.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        tx.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
