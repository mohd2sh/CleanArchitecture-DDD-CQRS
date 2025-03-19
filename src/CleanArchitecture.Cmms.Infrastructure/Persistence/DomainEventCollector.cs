using CleanArchitecture.Cmms.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Cmms.Infrastructure.Persistence;

public static class DomainEventCollector
{
    public static IReadOnlyCollection<IDomainEvent> CollectAndClear(this DbContext db)
    {
        var aggregates = db.ChangeTracker.Entries<IHasDomainEvents>().Select(e => e.Entity).Where(e => e.DomainEvents.Count > 0).ToList();
        var events = aggregates.SelectMany(a => a.DomainEvents).ToList();
        aggregates.ForEach(a => a.ClearDomainEvents());
        return events;
    }
}
