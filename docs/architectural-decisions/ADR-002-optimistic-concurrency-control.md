# ADR-002: Optimistic Concurrency Control with RowVersion Pattern

 **Author:** Mohammad Shakhtour  
**Status:** Accepted  

## Summary

This ADR documents the decision to implement optimistic concurrency control using SQL Server's `ROWVERSION` (timestamp) pattern to prevent race conditions and ensure data consistency in concurrent scenarios.

## Context

In a CMMS (Computerized Maintenance Management System), multiple users may simultaneously attempt to modify the same assets, work orders, or technicians. Without proper concurrency control, race conditions can lead to data inconsistency, lost updates, and business rule violations.

### Problem Scenario

Consider a critical race condition in work order creation:

1. **Process A** reads Asset X (Status: Active, RowVersion: 0x0000000000000123)
2. **Process B** reads Asset X (Status: Active, RowVersion: 0x0000000000000123)
3. **Process A** creates WorkOrder for Asset X, changes status to UnderMaintenance
4. **Process B** creates WorkOrder for Asset X, changes status to UnderMaintenance
5. **Process A** commits successfully (RowVersion: 0x0000000000000124)
6. **Process B** commits successfully (RowVersion: 0x0000000000000125)

**Result:** Two work orders exist for the same segment of time, violating the business rule that an asset can only be under maintenance once.

## Decision Drivers

- **Data Consistency:** Prevent race conditions that violate business rules
- **Performance:** Avoid blocking operations with pessimistic locking
- **Scalability:** Support concurrent user operations without deadlocks
- **User Experience:** Provide clear error messages for concurrency conflicts
- **Industry Standards:** Follow established patterns in .NET ecosystem

## Options Considered

### Option 1: No Concurrency Control
**Description:** Rely on application logic without database-level concurrency protection.

**Pros:**
- Simple implementation
- No additional database overhead

**Cons:**
- Race conditions and data inconsistency
- Lost updates in concurrent scenarios
- Business rule violations
- Unreliable system behavior

**Verdict:** **Rejected** - Fundamental data integrity issues

### Option 2: Pessimistic Locking
**Description:** Lock database rows during read operations until transaction completion.

**Implementation:**
```csharp
// Using SELECT ... FOR UPDATE (SQL Server: WITH (UPDLOCK, HOLDLOCK))
var asset = await _repository.GetByIdAsync(assetId, cancellationToken);
// Lock held until transaction commits
```

**Pros:**
- Guaranteed consistency
- No lost updates
- Simple conflict resolution

**Cons:**
- Poor performance under load
- Deadlock potential
- Reduced concurrency
- Poor user experience (long waits)
- Not suitable for web applications

**Verdict:** **Rejected** - Performance and scalability concerns

### Option 3: Optimistic Concurrency Control with RowVersion
**Description:** Use SQL Server's `ROWVERSION` (timestamp) columns to detect concurrent modifications.

**Implementation:**
```csharp
// Domain Entity
public abstract class AggregateRoot<TId> : AuditableEntity<TId>, IAggregateRoot
{
    [Timestamp]
    public byte[] RowVersion { get; protected set; } = default!;
}

// Entity Configuration
builder.Property(e => e.RowVersion)
    .IsRowVersion()
    .IsConcurrencyToken();
```

**Pros:**
- Excellent performance and scalability
- No deadlocks
- Industry standard pattern
- Clear conflict detection
- Works well with web applications

**Cons:**
- Requires retry logic for conflicts
- Slightly more complex error handling

**Verdict:** **Accepted** - Best balance of performance and consistency

### Option 4: Application-Level Versioning
**Description:** Implement custom versioning using integer counters or GUIDs.

**Pros:**
- Database agnostic
- Custom conflict resolution logic

**Cons:**
- More complex implementation
- Requires manual version management
- Potential for version conflicts
- Not leveraging database optimizations

**Verdict:** **Rejected** - Unnecessary complexity when database provides native support

## Decision

**Chosen Approach:** Optimistic Concurrency Control with SQL Server RowVersion

This approach provides the optimal balance between data consistency, performance, and maintainability for a CMMS system with concurrent user operations.

## Implementation Details

### 1. Domain Layer - Aggregate Root Configuration

All aggregate roots inherit concurrency control:

```csharp
// src/CleanArchitecture.Cmms.Domain/Abstractions/AggregateRoot.cs
using System.ComponentModel.DataAnnotations;

namespace CleanArchitecture.Cmms.Domain.Abstractions
{
    internal abstract class AggregateRoot<TId> : AuditableEntity<TId>, IAggregateRoot
    {
        [Timestamp]
        public byte[] RowVersion { get; protected set; } = default!;

        protected AggregateRoot() { }
        protected AggregateRoot(TId id) : base(id) { }
    }
}
```

### 2. Infrastructure Layer - Entity Configuration

RowVersion is configured as a concurrency token:

```csharp
// src/CleanArchitecture.Cmms.Infrastructure/Persistence/Configurations/AuditableEntityConfiguration.cs
internal abstract class AuditableEntityConfiguration<TEntity, TId> : IEntityTypeConfiguration<TEntity>
    where TEntity : AggregateRoot<TId>
{
    public void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.Property(e => e.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();
        
        // ... other audit properties
        ConfigureCore(builder);
    }
    
    protected abstract void ConfigureCore(EntityTypeBuilder<TEntity> builder);
}
```

### 3. Application Layer - Error Definition

Concurrency conflicts are represented as domain errors:

```csharp
// src/CleanArchitecture.Cmms.Application/Assets/AssetErrors.cs
[ApplicationError]
public static readonly Error ConcurrencyConflict = Error.Conflict(
    "Asset.ConcurrencyConflict",
    "Asset was modified by another user. Please refresh and try again.");
```

### 4. API Layer - Exception Handling

Concurrency exceptions are handled globally:

```csharp
// src/CleanArchitecture.Cmms.Api/Middlewares/ExceptionHandlingMiddleware.cs
private async Task HandleExceptionAsync(HttpContext context, Exception ex)
{
    switch (ex)
    {
        case DbUpdateConcurrencyException concurrencyEx:
            await WriteConcurrencyResponseAsync(context, concurrencyEx);
            break;
        // ... other cases
    }
}

private async Task WriteConcurrencyResponseAsync(HttpContext context, DbUpdateConcurrencyException ex)
{
    _logger.LogWarning(ex, "Concurrency conflict occurred.");
    
    var result = Result.Failure(AssetErrors.ConcurrencyConflict);

    context.Response.ContentType = "application/json";
    context.Response.StatusCode = (int)HttpStatusCode.Conflict;

    await context.Response.WriteAsync(JsonSerializer.Serialize(result, _jsonOptions));
}
```

## How It Works

### Race Condition Prevention

1. **Read Phase:** Entity loaded with current RowVersion
2. **Business Logic:** Domain rules validated against current state
3. **Update Phase:** EntityFramework compares RowVersion during SaveChanges()
4. **Conflict Detection:** If RowVersion changed, `DbUpdateConcurrencyException` thrown
5. **Error Handling:** User receives clear conflict message

### Example: Concurrent Work Order Creation

```csharp
// Process A and B both execute simultaneously
public async Task<Result<Guid>> Handle(CreateWorkOrderCommand request, CancellationToken cancellationToken)
{
    // 1. Create location from request data
    var location = Location.Create(request.Building, request.Floor, request.Room);
    
    // 2. Create work order
    var workOrder = WorkOrder.Create(request.AssetId, request.Title, location);
    await _workOrderRepository.AddAsync(workOrder, cancellationToken);
    
    // 3. Return result - SaveChanges happens in TransactionPipeline
    // WorkOrderCreatedEvent is raised automatically
    return workOrder.Id;
}

// Event Handler that runs after work order creation
public async Task Handle(WorkOrderCreatedEvent notification, CancellationToken cancellationToken)
{
    // 1. Load asset (RowVersion: 0x0000000000000123 for both processes)
    var asset = await _assetRepository.GetByIdAsync(notification.AssetId, cancellationToken);
    
    
    // 2. Set asset under maintenance
    asset.SetUnderMaintenance("Work order created", notification.CreatedBy, DateTime.UtcNow);
    
    // 3. No SaveChanges here - handled by TransactionPipeline
    // Process A succeeds, Process B gets DbUpdateConcurrencyException
}

// Pipeline execution order:
// 1. DomainEventsPipeline (runs event handlers)
// 2. TransactionPipeline (calls SaveChanges)
// Concurrency conflict occurs during SaveChanges in TransactionPipeline
```

## Future Considerations

### Microservices Migration
When migrating to microservices, consider:

1. **Event Sourcing:** Replace RowVersion with event versioning
2. **Distributed Locks:** Use Redis or database locks for critical sections
3. **Saga Pattern:** Handle distributed transactions across services
4. **Idempotency:** Ensure operations can be safely retried

### Alternative Databases
For non-SQL Server databases:

1. **PostgreSQL:** Use `xmin` system column or custom version fields
2. **MySQL:** Use custom integer version columns
3. **NoSQL:** Implement application-level versioning

## References

- [SQL Server RowVersion Documentation](https://docs.microsoft.com/en-us/sql/t-sql/data-types/rowversion-transact-sql)
- [Entity Framework Concurrency Tokens](https://docs.microsoft.com/en-us/ef/core/saving/concurrency)
- [Optimistic vs Pessimistic Locking](https://docs.microsoft.com/en-us/ef/core/saving/concurrency)
- [Domain-Driven Design by Eric Evans](https://www.domainlanguage.com/ddd/)
- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
