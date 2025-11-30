using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CleanArchitecture.Core.Infrastructure.Persistence.EfCore.Interceptors;

public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly Func<string?>? _getCurrentUserDelegate;

    public AuditableEntityInterceptor(
        IDateTimeProvider dateTimeProvider,
        Func<string?>? getCurrentUserDelegate = null)
    {
        _dateTimeProvider = dateTimeProvider;
        _getCurrentUserDelegate = getCurrentUserDelegate;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        UpdateAuditFields(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateAuditFields(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateAuditFields(DbContext? context)
    {
        if (context == null) return;

        var currentUser = _getCurrentUserDelegate?.Invoke() ?? "Default User";

        var entries = context.ChangeTracker.Entries();

        foreach (var entry in entries)
        {
            if (entry.Entity is not IAuditableEntity auditable)
                continue;

            if (entry.State == EntityState.Added)
                auditable.SetCreated(_dateTimeProvider.UtcNow, currentUser);

            if (entry.State == EntityState.Modified)
                auditable.SetLastModified(_dateTimeProvider.UtcNow, currentUser);
        }
    }
}
