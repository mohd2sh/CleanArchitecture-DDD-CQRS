using System;
using System.Threading;
using System.Threading.Tasks;
using CleanArchitecture.Outbox.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Outbox.Persistence;

internal sealed class EfCoreOutboxStore : ICustomOutboxStore
{
    private readonly OutboxDbContext _context;

    public EfCoreOutboxStore(OutboxDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        await _context.OutboxMessages.AddAsync(message, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<OutboxMessage?> GetNextUnprocessedMessageAsync(CancellationToken cancellationToken = default)
    {
        return await _context.OutboxMessages
            .FromSqlInterpolated($@"
                SELECT TOP (1) *
                FROM OutboxMessages WITH (UPDLOCK, ROWLOCK, READPAST)
                WHERE ProcessedAt IS NULL 
                  AND RetryCount < MaxRetries
                ORDER BY CreatedAt")
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task MarkAsProcessedAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        var message = await _context.OutboxMessages.FindAsync(new object[] { messageId }, cancellationToken);
        if (message != null)
        {
            message.ProcessedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task IncrementRetryAsync(Guid messageId, string error, CancellationToken cancellationToken = default)
    {
        var message = await _context.OutboxMessages.FindAsync(new object[] { messageId }, cancellationToken);

        if (message == null)
        {
            return;
        }

        message.RetryCount++;

        message.LastError = error.Length > 2000 ? error.Substring(0, 2000) : error;

        if (message.RetryCount >= message.MaxRetries)
        {
            MoveToDeadLetterAsync(message);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private void MoveToDeadLetterAsync(OutboxMessage message)
    {
        var deadLetterMessage = new DeadLetterMessage
        {
            Id = message.Id,
            EventType = message.EventType,
            Payload = message.Payload,
            CreatedAt = message.CreatedAt,
            MovedToDeadLetterAt = DateTime.UtcNow,
            RetryCount = message.RetryCount,
            LastError = message.LastError,
            MaxRetries = message.MaxRetries
        };

        _context.DeadLetterMessages.Add(deadLetterMessage);

        _context.OutboxMessages.Remove(message);
    }
}
