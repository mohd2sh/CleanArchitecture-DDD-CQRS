using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CleanArchitecture.Cmms.Infrastructure.Persistence.EfCore.Interceptors;

public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public AuditableEntityInterceptor(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    /// <summary>
    ///  Out of current scope but this would be dynamically set based on the current user context.
    ///  Like: IUserContext  IAuthService etc..
    /// </summary>
    private const string CurrentUser = "Default User";

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

        var entries = context.ChangeTracker.Entries();

        foreach (var entry in entries)
        {
            if (entry.Entity is not IAuditableEntity auditable)
                continue;

            if (entry.State == EntityState.Added)
                auditable.SetCreated(_dateTimeProvider.UtcNow, CurrentUser);

            if (entry.State == EntityState.Modified)
                auditable.SetLastModified(_dateTimeProvider.UtcNow, CurrentUser);
        }
    }
}
