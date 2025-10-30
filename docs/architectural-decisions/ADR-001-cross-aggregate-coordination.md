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

Eric Evans defines aggregates as *“a cluster of associated objects that we treat as a unit for the purpose of data changes”*  
(*Domain-Driven Design*).

He explains that an aggregate must maintain its own invariants independently —  
**one aggregate should not depend on the state of another to remain consistent.**  

This principle ensures each aggregate forms a clear consistency boundary and can evolve or persist independently without requiring coordination with others.

Vaughn Vernon reinforces this in Implementing Domain-Driven Design:

> **Vaughn Vernon (paraphrased): Aggregates should be kept small. When a business operation requires coordination across multiple aggregates, handle it through a Domain Service or an Application Service.**  

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

The command handler coordinates multiple aggregates to complete a workflow.
This approach is acceptable when the logic is straightforward and each aggregate can perform its work independently.

However, it can become problematic if the handler needs data from one aggregate to compute or validate something in another.
That usually means the aggregate boundaries are off, and the logic likely belongs inside a domain service or a single, larger aggregate.

**Problems:**
-  Tight coupling to multiple repositories
-  Harder to evolve
-  Cross-aggregate lookups often indicate poor domain design
-  Hard to test (requires mocking multiple repositories)
-  Business logic might leaks into application layer

**Verdict:**  Acceptable for simple use cases within one bounded context.

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
-  Difficult to evolve to async style microservices
-  Commands are meant for external requests, not internal coordination
-  Unclear transaction boundaries (who commits what?)

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

This is **more acceptable** than sending commands, but still introduces runtime coupling and potential consistency risks between bounded contexts.


**Problems:**
-  Still couples at query level
-  Potential race condition (data might change between query and command)

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
-  Requires careful modeling to avoid cross-context leakage

**When to Use:**
- Complex orchestration logic (if-then-else across aggregates)
- Operations requiring bidirectional data flow
- Pragmatic exception for tightly coupled operations

** Design Smell Warning:**

If you find yourself needing a computed value from one aggregate to create or modify another aggregate, this is a strong indicator of poor domain modeling.

**Better Approaches:**
- Include necessary data in the command (denormalization)
- Use domain events
- Rethink your aggregate boundaries

**Verdict:**  **Legitimate DDD Pattern** - Use for complex cases, but re-examine your model if you need cross-aggregate computed values

---

### Option 5: Domain Events (Synchronous And Asynchronous) 

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



**Pros:**
-  **Maintains aggregate boundaries** - Each aggregate only modifies itself
-  **Clean separation of concerns** - Each context reacts independently
-  **Single ACID transaction** - All handlers in same transaction
-  **Automatic rollback** - If any handler fails, entire operation rolls back
-  **Easy to test** - Mock event handlers independently
-  **Microservices-ready** - Just change to Integration Events + Outbox
-  **DDD-endorsed** - Explicitly recommended by Evans & Vernon
-  **Scalable** - Easy to add/remove event handlers
-  **Enforces design boundaries** - Prevents cross-aggregate computed value anti-pattern by making data flow explicit through events rather than queries

**Cons:**
-  More files (one handler per context)
-  Requires understanding event flow


**When to Use:**
- Default pattern for cross-aggregate coordination
- Operations that are independent side-effects
- When ACID consistency is required
- When bounded context boundaries must remain clean

**Verdict:**  **Recommended Primary Pattern**

---

## Decision

### Primary Pattern: Domain Events (Synchronous/Transactional)

**choose Domain Events as the primary coordination pattern.**

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
workOrder.Complete(); // → Event → Handlers
```

### Future Evolution

**Current (Monolith with single database):**
- Synchronous domain events
- Single ACID transaction
- All handlers in same process

**Future (Microservices):**
- Outbox published to Bus
- Saga introduced if necessary
- Eventual consistency between services

**Migration Path:**
```csharp
// Transactional
internal sealed class WorkOrderCompletedEventHandler : IDomainEventHandler<WorkOrderCompletedEvent>

// Tomorrow (microservices)
internal sealed class WorkOrderCompletedHandler
    : IIntegrationEventHandler<WorkOrderCompletedEvent>
// Infrastructure automatically uses Outbox + Message Bus
```


### Related Decisions
- ADR-003: Domain Events vs Integration Events — distinction between `IDomainEventHandler` and `IIntegrationEventHandler`.

---

## Architecture Tests

To enforce this decision, we implement architecture tests:

```csharp
[Fact]
public void CommandHandlers_Should_Use_Repository_From_Same_BoundedContext()
{
    
}
```

This test ensures handlers don't inject repositories from other contexts, forcing the use of events or services.

---

## Notes

This decision prioritizes **clean boundaries and microservices-readiness** over immediate simplicity. While handler orchestration might seem simpler initially, the event-driven approach provides:

1. Better long-term maintainability
2. Clear bounded context boundaries
3. Natural evolution path to distributed architecture
4. Testability and extensibility

---

