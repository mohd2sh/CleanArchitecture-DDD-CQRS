# ADR-003: Domain Events vs Integration Events

**Author:** Mohammad Shakhtour  
**Status:** Accepted  

## Context

### The Problem

In ADR-001, we established Domain Events as the primary pattern for cross-aggregate coordination. However, not all events have the same consistency and delivery requirements. Consider two scenarios in our CMMS system:

**Scenario 1: Work Order Creation**
1. User creates a work order for an asset
2. Asset status must immediately change to "Under Maintenance"
3. This status change must happen in the same transaction
4. If the work order creation fails, the asset status must not change

**Scenario 2: Work Order Completion Email**
1. Work order is completed successfully
2. System sends notification email to stakeholders
3. Email delivery can happen asynchronously
4. Email failure should not rollback the work order completion

**The Challenge/Need:** How do we handle these fundamentally different event processing requirements while maintaining clean architecture and preparing for future distributed system evolution?

### Why This Matters

The pattern extends to event processing strategies:

- **Transactional Events:** Must execute immediately within the same transaction (strong consistency)
- **Integration Events:** Can execute asynchronously outside the transaction (eventual consistency or no need for transactions)

---

## Decision Drivers

1. **Transaction Boundaries** - Some operations require ACID guarantees, others don't
2. **Consistency Requirements** - Critical state changes need strong consistency, notifications can be eventual
3. **System Resilience** - Non-critical operations shouldn't block or fail critical transactions
4. **Performance** - Asynchronous processing for long-running operations (emails, external APIs)
5. **Microservices Readiness** - Clear separation enables future distributed architecture
6. **Failure Isolation** - External system failures shouldn't affect core business operations

---

## Options Considered

### Option 1: Single Event Type (All Synchronous)

**Pattern:**
All events execute synchronously within the transaction. Handlers modify state, send emails, call external APIs - all in one transaction.

**Pros:**
- Simple mental model
- Immediate consistency for everything
- Single event handling mechanism

**Cons:**
- Long-running operations block transactions
- External service failures rollback business transactions
- Poor scalability (transaction holds locks during I/O)
- Cannot evolve to microservices easily
- Tight coupling to external systems

**Verdict:** Rejected - External failures affect core business operations

---

### Option 2: Single Event Type (All Asynchronous)

**Pattern:**
All events execute asynchronously outside the transaction. Even critical state changes happen via eventual consistency.

**Pros:**
- No transaction blocking
- Good scalability
- Loose coupling

**Cons:**
- Critical state changes not atomic with command
- Complex error handling for state modifications
- Race conditions between aggregates
- Violates business invariants temporarily
- Not suitable for operations requiring immediate consistency

**Verdict:** Rejected - Cannot guarantee critical business invariants

---

### Option 3: Two Distinct Event Handler Types (Chosen)

**Pattern:**
Separate interfaces for different consistency requirements:

- **IDomainEventHandler:** Synchronous execution within transaction
- **IIntegrationEventHandler:** Asynchronous execution via outbox pattern

**Decision Criteria:**
- Modifies aggregate state → Domain Event Handler
- Sends notifications, calls external systems → Integration Event Handler
- Requires immediate consistency → Domain Event Handler
- Can tolerate eventual consistency → Integration Event Handler

**Pros:**
- Clear separation of concerns at compile-time
- Strong consistency where needed, eventual consistency where acceptable
- Failure isolation (external failures don't affect core operations)
- Natural evolution to microservices
- Type-safe distinction

**Cons:**
- Developers must understand two handler types (Arch unit test and the project structure can help with this)
- Need to choose correct handler type for each event

**Verdict:** Accepted - Balances consistency, performance, and future evolution

---

### Option 4: Attribute-Based Distinction

**Pattern:**
Single handler interface with attributes marking execution strategy:
```csharp
[Transactional]
public class AssetStatusEventHandler : IEventHandler<WorkOrderCreated> { }

[Asynchronous]
public class EmailEventHandler : IEventHandler<WorkOrderCompleted> { }
```

**Pros:**
- Single handler interface
- Metadata-driven behavior

**Cons:**
- Reflection overhead
- Easy to forget attribute (runtime error)
- No compile-time safety
- Less explicit in code

**Verdict:** Rejected 

---

## Decision

**Implement two distinct event handler interfaces:**

### IDomainEventHandler<TEvent>

**Purpose:** Handle events that require strong consistency and immediate state changes within the same transaction.

**Characteristics:**
- Runs within the same database transaction
- Can modify aggregate state via repositories
- Changes committed atomically with the command
- Failure causes transaction rollback

**Use Cases:**
- Cross-aggregate state synchronization
- Business rule enforcement across aggregates
- Critical operations requiring ACID guarantees

**Example:**
```csharp
// WorkOrder created → Asset must be set to "Under Maintenance"
public class WorkOrderCreatedEventHandler : IDomainEventHandler<WorkOrderCreatedEvent>
{
    public async Task Handle(WorkOrderCreatedEvent @event, CancellationToken ct)
    {
        var asset = await _assetRepository.GetByIdAsync(@event.AssetId, ct);
        asset.SetUnderMaintenance();
        // Saved in same transaction
        //if any domain exceptions , it will be auto rollback
    }
}
```

### IIntegrationEventHandler<TEvent>

**Purpose:** Handle events that can execute asynchronously with eventual consistency, typically for cross-boundary communication.

**Characteristics:**
- Executes asynchronously (outside transaction)
- Guaranteed delivery via outbox pattern
- Automatic retry on failure
- Survives application restarts
- Does not modify core aggregate state

**Use Cases:**
- Sending notifications (email, SMS, push)
- Publishing to message bus for external systems
- Logging and auditing
- Analytics and reporting
- Cross-service communication (future microservices)

**Example:**
```csharp
// WorkOrder completed → Send notification email
public class EmailWorkOrderCompletedHandler : IIntegrationEventHandler<WorkOrderCompletedEvent>
{
    public async Task Handle(WorkOrderCompletedEvent @event, CancellationToken ct)
    {
        await _emailService.SendCompletionNotification(@event.WorkOrderId);
        // Email failure won't rollback work order completion
    }
}
```

---

### Key Design Points

**Transactional Consistency:**
- Domain event handlers share the transaction
- All state changes atomic
- Rollback affects everything

**Guaranteed Delivery:**
- Integration events written to outbox in same transaction
- If transaction commits, events will eventually be processed
- Survives application crashes and restarts

**Failure Isolation:**
- Integration event handler failures don't affect core operations
- Automatic retry with configurable attempts
- Dead letter queue for persistent failures (future)

---

## Future Evolution

### Default Implementation (Single-Process Setup)

**Domain Events:**
- Synchronous execution within process
- Shared database transaction
- Direct repository access

**Integration Events:**
- Outbox table in same database
- Background processor in same process
- In-process handler invocation

### Microservices Deployment

**Domain Events:**
- Remain internal to each service
- Service-specific database transactions
- No cross-service synchronous events

**Integration Events:**
- Outbox processor publishes to message bus (RabbitMQ, Azure Service Bus)
- Cross-service communication via async messaging
- Consumer services subscribe to relevant events
- Schema registry for event contracts
- Introduce Saga and Compensation messages

**No Business Code Changes Required:** Handler interfaces remain identical—only infrastructure configuration changes. When migrating to fully distributed async architectures, compensation events or Saga may be required for distributed transaction handling.

---

## Guidelines

### When to Use IDomainEventHandler

Use when the event handler:
- Modifies aggregate state in another bounded context
- Enforces business rules across aggregates
- Requires immediate consistency
- Must participate in the same transaction
- Failure should rollback the entire operation

### When to Use IIntegrationEventHandler

Use when the event handler:
- Sends notifications (email, SMS, push)
- Calls external APIs or services
- Performs logging or auditing
- Updates read models or caches
- Publishes to message bus
- Can tolerate eventual consistency
- Failure should not affect core operation

### Decision Flowchart

```
Does the handler modify aggregate state?
├─ Yes → Does it require immediate consistency?
│         ├─ Yes → IDomainEventHandler
│         └─ No  → IIntegrationEventHandler
│
└─ No  → Is it a notification or external call?
          └─ Yes → IIntegrationEventHandler
```

---

## Notes

This decision reflects the reality that different operations have different consistency requirements. By making this explicit at the type system level, we provide clear guidance to developers and enable the system to evolve naturally toward a distributed architecture.

---

