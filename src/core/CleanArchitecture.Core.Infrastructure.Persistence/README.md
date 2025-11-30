# CleanArchitecture.Core.Infrastructure.Persistence

Core infrastructure persistence implementations for Entity Framework Core including unit of work, transactions, criteria extensions, domain event collection, and audit interceptors.

## Purpose

This package provides base Entity Framework Core implementations of persistence abstractions including unit of work pattern, transaction management, criteria query extensions, domain event collection, and automatic audit field management. It works with CleanArchitecture.Core.Application to provide a complete persistence foundation for Clean Architecture applications.

## Installation

```bash
dotnet add package Mohd2sh.CleanArchitecture.Core.Infrastructure.Persistence
```

## Usage


### Register Unit of Work

```csharp
services.AddScoped<IUnitOfWork>(sp => 
    new EfUnitOfWork(sp.GetRequiredService<ApplicationDbContext>()));
```

### Using EfCriteria Extensions

```csharp
public class ProductRepository
{
    private readonly DbContext _db;
    
    public async Task<PaginatedList<Product>> GetProductsAsync(
        int pageNumber, 
        int pageSize, 
        string? searchTerm)
    {
        var criteria = Criteria<Product>.New()
            .Where(p => searchTerm == null || p.Name.Contains(searchTerm))
            .OrderByAsc(p => p.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Build();
        
        var (query, totalCount) = await _db.Set<Product>()
            .ApplyWithCountAsync(criteria);
        
        var items = await query.ToListAsync();
        
        return new PaginatedList<Product>(items, totalCount, pageNumber, pageSize);
    }
}
```

### Domain Event Collection

```csharp
// After SaveChangesAsync, collect domain events
var domainEvents = _dbContext.CollectAndClear();

// Domain events are automatically cleared from aggregates
// Dispatch events through your event dispatcher
foreach (var domainEvent in domainEvents)
{
    await _domainEventDispatcher.PublishAsync(domainEvent);
}
```

### Auditable Entity Interceptor

```csharp
// Register the interceptor in your DbContext configuration
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder.AddInterceptors(new AuditableEntityInterceptor(
        _dateTimeProvider,
        () => _currentUserService.GetCurrentUserId()));
}

// Entities implementing IAuditableEntity will automatically have
// CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy set
```

## Dependencies

Requires CleanArchitecture.Core.Application and Microsoft.EntityFrameworkCore.

## Repository

GitHub: https://github.com/mohd2sh/CleanArchitecture-DDD-CQRS

## License

MIT
