using CleanArchitecture.Cmms.Application.Abstractions.Persistence;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Cmms.Application.Behaviors;

internal sealed class TransactionCommandPipeline<TCommand, TResult>
    : ICommandPipeline<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<TransactionCommandPipeline<TCommand, TResult>> _logger;

    public TransactionCommandPipeline(IUnitOfWork uow, ILogger<TransactionCommandPipeline<TCommand, TResult>> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<TResult> Handle(TCommand request, RequestHandlerDelegate<TResult> next, CancellationToken cancellationToken)
    {
        await using var transaction = await _uow.BeginTransactionAsync(cancellationToken);

        try
        {
            var result = await next();

            await _uow.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction failed for {Command}", typeof(TCommand).Name);
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
