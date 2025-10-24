# ADR-001: Cross-Aggregate Coordination Pattern

**Author:** Mohammad Shakhtour  
**Status:** Accepted  

## Context

### The Problem

In a CMMS (Computerized Maintenance Management System), operations frequently span multiple aggregate roots across different bounded contexts. Consider the `CompleteWorkOrder` command:

1. **WorkOrder aggregate** must be marked as completed
2. **Asset aggregate** must transition from "Under Maintenance" to "Operational"  
3. **Technician aggregate** must record the completed assignment and free up capacity

**Challenge:** How do we coordinate these operations while maintaining:
- Aggregate boundary integrity (DDD principle)
- ACID transaction guarantees
- Clean architecture separation
- Ability to evolve toward microservices

### Why This Matters

Eric Evans defines aggregates as *"a cluster of associated objects that we treat as a unit for the purpose of data changes"* (Domain-Driven Design, Chapter 6). The fundamental rule:

> **"One aggregate should not depend on the state of another aggregate to maintain its invariants."**  
> — Eric Evans, Domain-Driven Design (2003)

Vaughn Vernon reinforces this in Implementing Domain-Driven Design:

> **"Aggregates should be designed small. When multiple aggregates are needed for a single command, use a Domain Service or Application Service."**  
> — Vaughn Vernon, IDDD, Chapter 10

The question becomes: **Which coordination pattern best maintains these principles while remaining pragmatic?**

---

## Decision Drivers

1. **DDD Principles** - Respect aggregate boundaries and invariants
2. **Clean Architecture** - Dependency rules, layer separation  
3. **Testability** - Easy to unit test in isolation
4. **Maintainability** - Clear, understandable coordination
5. **Microservices-Ready** - Can evolve to distributed system
6. **Transaction Consistency** - ACID guarantees when needed
7. **Simplicity** - Occam's Razor - simplest solution that works

---

## Options Considered

### Option 1: Command Handler Orchestration 

**Pattern:**
```csharp
public class CompleteWorkOrderCommandHandler
{
    private readonly IRepository<WorkOrder, Guid> _workOrderRepo;
    private readonly IRepository<Asset, Guid> _assetRepo;
    private readonly IRepository<Technician, Guid> _technicianRepo;
    
    public async Task<Result> Handle(CompleteWorkOrderCommand request, CancellationToken ct)
    {
        var workOrder = await _workOrderRepo.GetByIdAsync(request.WorkOrderId, ct);
        var asset = await _assetRepo.GetByIdAsync(workOrder.AssetId, ct);
        var technician = await _technicianRepo.GetByIdAsync(workOrder.TechnicianId, ct);
        
        // Handler orchestrates all aggregates directly
        workOrder.Complete();
        asset.CompleteMaintenance();
        technician.CompleteAssignment(workOrder.Id);
        
        return Result.Success();
    }
}
```

**Analysis:**

This is the **Transaction Script** anti-pattern. Martin Fowler describes it:

> **"Transaction Script organizes business logic by procedures where each procedure handles a single request from the presentation. Most business applications can be thought of as a series of transactions... The dominant pattern is to have one procedure for each action."**  
> — Martin Fowler, Patterns of Enterprise Application Architecture (2002)

**Why It's Problematic:**

Vaughn Vernon explicitly warns against this:

> **"Avoid putting business logic in Application Services. Application Services should be thin and delegate to Domain Services or Aggregates."**  
> — Vaughn Vernon, IDDD, Chapter 4

**Problems:**
-  Violates Single Responsibility Principle (handler does too much)
-  Command handler becomes a "god object"
-  Violates bounded context boundaries (handler knows about all contexts)
-  Hard to test (requires mocking multiple repositories)
-  Business logic leaks into application layer
-  Violates architecture tests (cross-context repository access)

**Verdict:**  **Anti-Pattern** - Not DDD-aligned

---

### Option 2: Mediator.Send(Command) from Handler 

**Pattern:**
```csharp
public class CompleteWorkOrderCommandHandler
{
    private readonly IRepository<WorkOrder, Guid> _workOrderRepo;
    private readonly IMediator _mediator;
    
    public async Task<Result> Handle(CompleteWorkOrderCommand request, CancellationToken ct)
    {
        var workOrder = await _workOrderRepo.GetByIdAsync(request.WorkOrderId, ct);
        workOrder.Complete();
        
        // Send commands to other contexts
        await _mediator.Send(new CompleteAssetMaintenanceCommand(workOrder.AssetId), ct);
        await _mediator.Send(new CompleteAssignmentCommand(workOrder.TechnicianId, workOrder.Id), ct);
        
        return Result.Success();
    }
}
```

**Analysis:**

This creates **tight command-level coupling** between bounded contexts.

**Problems:**
-  Violates bounded context autonomy
-  Creates synchronous coupling at command level
-  Handler must know about other contexts' command contracts
-  Difficult to evolve to microservices (would need distributed transactions)
-  Commands are meant for external requests, not internal coordination
-  Unclear transaction boundaries (who commits what?)

**Vaughn Vernon on Cross-Context Commands:**

> **"Commands should not be used for communication between bounded contexts. Use Integration Events for that purpose."**  
> — Vaughn Vernon, IDDD, Chapter 13

**Verdict:**  **Not Recommended** - Violates bounded context independence

---

### Option 3: Mediator.Send(Query) from Handler 

**Pattern:**
```csharp
public class AssignTechnicianCommandHandler
{
    private readonly IRepository<WorkOrder, Guid> _workOrderRepo;
    private readonly IMediator _mediator;
    
    public async Task<Result> Handle(AssignTechnicianCommand request, CancellationToken ct)
    {
        // Query another context for validation (read-only)
        var technicianAvailability = await _mediator.Send(
            new GetTechnicianAvailabilityQuery(request.TechnicianId), ct);
        
        if (!technicianAvailability.IsAvailable)
            return Result.Failure(TechnicianErrors.NotAvailable);
        
        var workOrder = await _workOrderRepo.GetByIdAsync(request.WorkOrderId, ct);
        workOrder.AssignTechnician(request.TechnicianId);
        
        return Result.Success();
    }
}
```

**Analysis:**

This is **more acceptable** than sending commands, but still creates coupling.

**When It's Acceptable:**
- ✓ Read-only access (no state modification)
- ✓ Validation purposes only
- ✓ Can easily become API call in microservices

**Problems:**
-  Still couples at query level
-  Potential race condition (data might change between query and command)
-  Not truly needed if events handle validation

**Verdict:**  **Acceptable but not ideal** - Creates read coupling

---

### Option 4: Domain Service 

**Pattern:**
```csharp
// Domain/Application Service Interface
public interface IWorkOrderCompletionService
{
    Task<Result> CompleteWorkOrderAsync(Guid workOrderId, string notes);
}

// Implementation
internal class WorkOrderCompletionService : IWorkOrderCompletionService
{
    private readonly IRepository<WorkOrder, Guid> _workOrderRepo;
    private readonly IRepository<Asset, Guid> _assetRepo;
    private readonly IRepository<Technician, Guid> _technicianRepo;
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task<Result> CompleteWorkOrderAsync(Guid workOrderId, string notes)
    {
        // Load all aggregates
        var workOrder = await _workOrderRepo.GetByIdAsync(workOrderId);
        if (workOrder is null) return Result.Failure(WorkOrderErrors.NotFound);
        
        var asset = await _assetRepo.GetByIdAsync(workOrder.AssetId);
        var technician = await _technicianRepo.GetByIdAsync(workOrder.TechnicianId.Value);
        
        // Orchestrate the operation
        var result = workOrder.Complete(notes);
        if (result.IsFailure) return result;
        
        asset.CompleteMaintenance(DateTime.UtcNow, notes);
        technician.CompleteAssignment(workOrder.Id, DateTime.UtcNow);
        
        // Single transaction commits all changes
        await _unitOfWork.SaveChangesAsync();
        
        return Result.Success();
    }
}

// Command Handler delegates to service
public class CompleteWorkOrderCommandHandler
{
    private readonly IWorkOrderCompletionService _completionService;
    
    public async Task<Result> Handle(CompleteWorkOrderCommand request, CancellationToken ct)
    {
        return await _completionService.CompleteWorkOrderAsync(
            request.WorkOrderId, 
            request.Notes);
    }
}
```

**Analysis:**

This is **explicitly endorsed** by DDD authorities.

**Eric Evans on Domain Services:**

> **"Some concepts from the domain aren't natural to model as objects. Forcing the required domain functionality to be the responsibility of an ENTITY or VALUE either distorts the definition of a model-based object or adds meaningless artificial objects. A SERVICE is an operation offered as an interface that stands alone in the model, without encapsulating state... When a significant process or transformation in the domain is not a natural responsibility of an ENTITY or VALUE OBJECT, add an operation to the model as a standalone interface declared as a SERVICE."**  
> — Eric Evans, Domain-Driven Design, Chapter 5

**Vaughn Vernon on Application Services:**

> **"Use a Domain Service when an operation crosses multiple aggregates. The Domain Service will coordinate the operation by loading the necessary aggregates, invoking behavior on them, and committing the Unit of Work."**  
> — Vaughn Vernon, IDDD, Chapter 7

**Pros:**
-  DDD-legitimate pattern (endorsed by Evans & Vernon)
-  Single ACID transaction
-  Explicit orchestration logic
-  Can handle complex coordination
-  Testable in isolation
-  Command handlers stay thin

**Cons:**
-  Couples bounded contexts in the service
-  Service depends on multiple aggregates
-  Harder to extract to microservices (need to split service)
-  Can become a "transaction script" if not careful

**When to Use:**
- Complex orchestration logic (if-then-else across aggregates)
- Operations requiring bidirectional data flow
- Need computed value from one aggregate for another
- Pragmatic exception for tightly coupled operations

**Verdict:**  **Legitimate DDD Pattern** - Use for complex cases

---

### Option 5: Domain Events (Synchronous/Transactional) 

**Pattern:**
```csharp
// Command Handler - Only touches own aggregate
public class CompleteWorkOrderCommandHandler
{
    private readonly IRepository<WorkOrder, Guid> _workOrderRepo;
    
    public async Task<Result> Handle(CompleteWorkOrderCommand request, CancellationToken ct)
    {
        var workOrder = await _workOrderRepo.GetByIdAsync(request.WorkOrderId, ct);
        if (workOrder is null) return Result.Failure(WorkOrderErrors.NotFound);
        
        workOrder.Complete(request.Notes); // Raises WorkOrderCompletedEvent
        
        return Result.Success();
    }
}

// Event Handlers in other bounded contexts
namespace Application.Assets.Events
{
    [TransactionalEvent] // Executes in same transaction
    internal class WorkOrderCompletedEventHandler 
        : INotificationHandler<DomainEventNotification<WorkOrderCompletedEvent>>
    {
        private readonly IRepository<Asset, Guid> _assetRepo;
        
        public async Task Handle(
            DomainEventNotification<WorkOrderCompletedEvent> notification, 
            CancellationToken ct)
        {
            var asset = await _assetRepo.GetByIdAsync(notification.DomainEvent.AssetId, ct);
            if (asset is null) 
                throw new DomainException(AssetErrors.NotFound);
            
            asset.CompleteMaintenance(DateTime.UtcNow, notification.DomainEvent.Notes);
            // Saved by transaction pipeline
        }
    }
}

namespace Application.Technicians.Events
{
    [TransactionalEvent]
    internal class WorkOrderCompletedEventHandler 
        : INotificationHandler<DomainEventNotification<WorkOrderCompletedEvent>>
    {
        private readonly IRepository<Technician, Guid> _technicianRepo;
        
        public async Task Handle(
            DomainEventNotification<WorkOrderCompletedEvent> notification, 
            CancellationToken ct)
        {
            if (!notification.DomainEvent.TechnicianId.HasValue) return;
            
            var technician = await _technicianRepo.GetByIdAsync(
                notification.DomainEvent.TechnicianId.Value, ct);
            
            if (technician is null) return; // Idempotent
            
            technician.CompleteAssignment(
                notification.DomainEvent.WorkOrderId, 
                DateTime.UtcNow);
        }
    }
}
```

**Pipeline Execution:**
```
1. TransactionCommandPipeline opens transaction
2. DomainEventsPipeline executes
3. Command handler: workOrder.Complete() → raises event
4. Domain events published (inside transaction)
5. Asset event handler executes
6. Technician event handler executes
7. SaveChanges() commits all changes
8. Transaction commits
   
If any handler fails → entire transaction rolls back 
```

**Analysis:**

This is the **core DDD event-driven pattern**.

**Eric Evans on Domain Events:**

> **"Model information about activity in the domain as a series of discrete events. Represent each event as a domain object... A domain event is a full-fledged part of the domain model, a representation of something that happened in the domain. Ignore irrelevant domain activity while making explicit the events that the domain experts want to track or be notified of, or which are associated with state change in other model objects."**  
> — Eric Evans, Domain-Driven Design Reference (2014)

**Vaughn Vernon on Domain Events:**

> **"Modeling events explicitly in the domain helps the team understand the business domain better. Use Domain Events to capture an occurrence of something that happened in the domain. Design events as immutable objects. Publish events from aggregates that undergo state changes. Other aggregates in the same Bounded Context can subscribe to these events."**  
> — Vaughn Vernon, IDDD, Chapter 8

**Udi Dahan (NServiceBus creator):**

> **"Don't let aggregates directly call each other. Instead, have them publish events that other aggregates subscribe to. This keeps them decoupled and makes the system more maintainable."**  
> — Udi Dahan, "Domain Events – Salvation" (2009)

**Pros:**
-  **Maintains aggregate boundaries** - Each aggregate only modifies itself
-  **Clean separation of concerns** - Each context reacts independently
-  **Single ACID transaction** - All handlers in same transaction
-  **Automatic rollback** - If any handler fails, entire operation rolls back
-  **Easy to test** - Mock event handlers independently
-  **Microservices-ready** - Just change to Integration Events + Outbox
-  **DDD-endorsed** - Explicitly recommended by Evans & Vernon
-  **Scalable** - Easy to add/remove event handlers

**Cons:**
-  More files (one handler per context)
-  Requires understanding event flow
-  All handlers must succeed (coupled at runtime)
-  Cannot return values from event handlers

**When to Use:**
- Default pattern for cross-aggregate coordination
- Operations that are independent side-effects
- When ACID consistency is required
- When bounded context boundaries must remain clean

**Verdict:**  **Recommended Primary Pattern**

---

### Option 6: Integration Events + Outbox (Asynchronous) 

**Pattern:**
```csharp
// Command Handler
public class CompleteWorkOrderCommandHandler
{
    private readonly IRepository<WorkOrder, Guid> _workOrderRepo;
    
    public async Task<Result> Handle(CompleteWorkOrderCommand request, CancellationToken ct)
    {
        var workOrder = await _workOrderRepo.GetByIdAsync(request.WorkOrderId, ct);
        workOrder.Complete(request.Notes);
        
        // Raises WorkOrderCompletedIntegrationEvent
        // Published to Outbox after transaction commits
        
        return Result.Success();
    }
}

// Background worker processes Outbox
// Event handlers in other services/modules react asynchronously
```

**Analysis:**

This is for **microservices or asynchronous processing**.

**When to Use:**
- Microservices architecture
- Long-running processes
- Can tolerate eventual consistency
- Need independent scaling of services

**Pros:**
-  True bounded context independence
-  Can scale services separately
-  Non-blocking operations
-  Resilient to failures (retry mechanism)

**Cons:**
-  Eventual consistency (not immediate)
-  Requires message bus infrastructure
-  Complex error handling
-  Need Outbox pattern for reliability
-  Harder to debug

**Verdict:**  **Future Enhancement** - Not needed for monolith now

---

### Option 7: Saga Pattern 

**Pattern:**
Distributed transaction coordinator across microservices with compensating transactions.

**Analysis:**

This is **only for microservices** with distributed transactions.

**When to Use:**
- Microservices with separate databases
- Long-running business processes
- Need compensation logic for failures

**Verdict:**  **Microservices Only** - Not applicable to monolith

---

## Decision

### Primary Pattern: Domain Events (Synchronous/Transactional)

**We choose Domain Events as the primary coordination pattern.**

### Rationale

1. **DDD-Aligned:** Explicitly endorsed by Eric Evans and Vaughn Vernon
2. **Clean Boundaries:** Each bounded context modifies only its own aggregates
3. **ACID Guarantees:** All operations in single transaction
4. **Testable:** Easy to unit test handlers independently
5. **Microservices-Ready:** Natural evolution to Integration Events + Outbox
6. **Maintainable:** Clear event flow, easy to understand
7. **Scalable:** Easy to add new event handlers without changing core logic

### Exception: Domain Services for Complex Cases

For operations with **complex orchestration logic** or **bidirectional data flow**, we allow Domain Services as a pragmatic exception:

```csharp
// 90% of cases: Events
workOrder.Complete(); // → Event → Handlers

// 10% of cases: Domain Service
await _complexOperationService.ExecuteAsync(...);
```

### Future Evolution

**Current (Monolith with single database):**
- Synchronous domain events
- Single ACID transaction
- All handlers in same process

**Future (Microservices):**
- Change marker: `[TransactionalEvent]` → `[IntegrationEvent]`
- Add Outbox Pattern infrastructure
- Background worker processes events
- Eventual consistency between services

**Migration Path:**
```csharp
// Today
[TransactionalEvent]
public sealed class WorkOrderCompletedEvent : IDomainEvent { }

// Tomorrow (microservices)
[IntegrationEvent]
public sealed class WorkOrderCompletedEvent : IIntegrationEvent { }
// Infrastructure automatically uses Outbox + Message Bus
```

---

## Consequences

### Positive

-  **Clean Architecture:** Respects dependency rules and layer separation
-  **Bounded Context Integrity:** No cross-context repository access from handlers
-  **Transaction Safety:** ACID guarantees with automatic rollback
-  **Test Coverage:** Easy to unit test each handler independently
-  **Evolvability:** Can transition to microservices with minimal changes
-  **Documentation:** Event names clearly document what happens in the system
-  **Extensibility:** New side effects added by adding event handlers

### Negative

-  **More Files:** Each cross-aggregate operation needs multiple event handlers
-  **Learning Curve:** Team must understand event-driven architecture
-  **Debugging:** Event chains harder to debug than sequential code
-  **Runtime Coupling:** All handlers must succeed (but this ensures consistency)

### Risks Mitigated

-  Prevents "Big Ball of Mud" through clean boundaries
-  Prevents bounded context corruption
-  Enables independent evolution of contexts
-  Supports future microservices migration

---

## Implementation Guidelines

### Rule 1: Command Handlers Modify Only Their Aggregate

```csharp
//  GOOD
public class CompleteWorkOrderCommandHandler
{
    private readonly IRepository<WorkOrder, Guid> _workOrderRepo;
    
    public async Task<Result> Handle(...)
    {
        var workOrder = await _workOrderRepo.GetByIdAsync(...);
        workOrder.Complete(); // Raises event
        return Result.Success();
    }
}

//  BAD
public class CompleteWorkOrderCommandHandler
{
    private readonly IRepository<WorkOrder, Guid> _workOrderRepo;
    private readonly IRepository<Asset, Guid> _assetRepo; //  Wrong!
    private readonly IRepository<Technician, Guid> _technicianRepo; //  Wrong!
}
```

### Rule 2: Event Handlers React in Their Own Context

```csharp
//  GOOD - Asset context owns Asset updates
namespace Application.Assets.Events
{
    internal class WorkOrderCompletedEventHandler 
    {
        private readonly IRepository<Asset, Guid> _assetRepo;
        // Only Asset repository
    }
}
```

### Rule 3: Mark Transactional Events

```csharp
[TransactionalEvent] // Executes in same transaction
public sealed class WorkOrderCompletedEvent : IDomainEvent
{
    public Guid WorkOrderId { get; }
    public Guid AssetId { get; }
    public Guid? TechnicianId { get; }
    public string Notes { get; }
    
    // Immutable event
}
```

### Rule 4: Use Domain Service for Complex Coordination Only

```csharp
// When event chain becomes too complex:
public interface IComplexOperationService
{
    Task<Result> ExecuteAsync(...);
}

// Command handler delegates
public class CommandHandler
{
    private readonly IComplexOperationService _service;
    
    public async Task<Result> Handle(...)
    {
        return await _service.ExecuteAsync(...);
    }
}
```

### Rule 5: Make Event Handlers Idempotent

```csharp
public async Task Handle(
    DomainEventNotification<WorkOrderCompletedEvent> notification, 
    CancellationToken ct)
{
    var asset = await _assetRepo.GetByIdAsync(notification.DomainEvent.AssetId, ct);
    
    // Idempotent check
    if (asset is null || asset.Status == AssetStatus.Operational)
        return; // Already processed or doesn't exist
    
    asset.CompleteMaintenance(...);
}
```

---

## Architecture Tests

To enforce this decision, we implement architecture tests:

```csharp
[Fact]
public void CommandHandlers_Should_Use_Repository_From_Same_BoundedContext()
{
    var handlers = Types
        .InAssembly(ApplicationAssembly)
        .That()
        .ImplementInterface(typeof(ICommandHandler<,>))
        .GetTypes();

    var violations = new List<string>();

    foreach (var handler in handlers.Where(t => !t.IsAbstract))
    {
        var handlerContext = ExtractBoundedContext(handler.Namespace, "Application");
        
        var repositories = GetInjectedRepositories(handler);
        
        foreach (var (aggregateType, _) in repositories)
        {
            var aggregateContext = ExtractBoundedContext(aggregateType.Namespace, "Domain");
            
            if (!string.Equals(handlerContext, aggregateContext, StringComparison.Ordinal))
            {
                violations.Add(
                    $"{handler.Name} uses IRepository<{aggregateType.Name}> " +
                    $"from context '{aggregateContext}' (handler context: '{handlerContext}')");
            }
        }
    }

    Assert.True(!violations.Any(), 
        "CommandHandlers must use IRepository of their own bounded context:\n" + 
        string.Join("\n", violations));
}
```

This test ensures handlers don't inject repositories from other contexts, forcing the use of events or services.

---

## Examples from Production DDD Systems

### Kamil Grzybek - Modular Monolith with DDD

[GitHub Repository](https://github.com/kgrzybek/modular-monolith-with-ddd)

- Uses **Domain Events within modules** (synchronous)
- Uses **Integration Events between modules** (asynchronous with Outbox)
- Uses **Domain Services** for complex coordination

### Microsoft eShopOnContainers

- Uses **Integration Events** between microservices
- Event Bus with RabbitMQ
- Eventual consistency model

---

## References

### Books

1. **Evans, Eric.** *Domain-Driven Design: Tackling Complexity in the Heart of Software.* Addison-Wesley, 2003.
   - Chapter 5: "A Model Expressed in Software" (Services)
   - Chapter 6: "The Life Cycle of a Domain Object" (Aggregates)

2. **Vernon, Vaughn.** *Implementing Domain-Driven Design.* Addison-Wesley, 2013.
   - Chapter 7: "Services"
   - Chapter 8: "Domain Events"
   - Chapter 10: "Aggregates"

3. **Fowler, Martin.** *Patterns of Enterprise Application Architecture.* Addison-Wesley, 2002.
   - Chapter on "Transaction Script"
   - Chapter on "Domain Model"

4. **Evans, Eric.** *Domain-Driven Design Reference.* Dog Ear Publishing, 2014.
   - Section on "Domain Events"

### Articles

1. **Dahan, Udi.** "Domain Events – Salvation." UdiDahan.com, 2009.  
   <https://udidahan.com/2009/06/14/domain-events-salvation/>

2. **Grzybek, Kamil.** "Modular Monolith: Domain-Centric Design." kamilgrzybek.com  
   <https://www.kamilgrzybek.com/blog/modular-monolith-domain-centric-design>

### Code Examples

1. **Grzybek, Kamil.** *Modular Monolith with DDD.* GitHub, 2024.  
   <https://github.com/kgrzybek/modular-monolith-with-ddd>

---

## Related Decisions

- ADR-002: Optimistic Concurrency Control (RowVersion pattern)
- ADR-003: Pipeline Order (Transaction wraps Domain Events)

---

## Notes

This decision prioritizes **clean boundaries and microservices-readiness** over immediate simplicity. While handler orchestration might seem simpler initially, the event-driven approach provides:

1. Better long-term maintainability
2. Clear bounded context boundaries
3. Natural evolution path to distributed architecture
4. Testability and extensibility

The decision aligns with both **tactical DDD patterns** (aggregates, events) and **strategic DDD** (bounded contexts, context mapping).

---

