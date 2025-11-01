# ADR-004: Outbox Pattern for Guaranteed Event Delivery

**Author:** Mohammad Shakhtour  
**Status:** Accepted  

## Context

### The Problem

ADR-003 established that we use `IIntegrationEventHandler` for asynchronous event processing outside the transaction scope and in async way. But we also need to **guarantee that integration events are reliably delivered even in the face of system failures?**

Consider this failure scenario:

```
1. Transaction begins
2. Work order completed (database updated)
3. Transaction commits successfully
4. Send email notification via external API
5. Application crashes before email is sent
```

**Result:** Work order is completed in the database, but the notification email is never sent and there's no record that it should have been.

### The Dual-Write Problem

This is known as the **dual-write problem** in distributed systems:

> **"If you need to update two different systems (a database and a message broker), you cannot make both updates atomically. One might succeed while the other fails."**  
> — Chris Richardson, Microservices Patterns (2018)

**Traditional Approaches Fail:**

**Approach 1: Publish then Commit**
```
1. Publish event to message bus
2. Commit database transaction
Problem: If database commit fails, event already published
```

**Approach 2: Commit then Publish**
```
1. Commit database transaction  
2. Publish event to message bus
Problem: If publish fails or app crashes, event is lost
```

**Approach 3: Two-Phase Commit**
```
1. Prepare both database and message bus
2. Commit both atomically
Problem: Complex, poor performance, requires distributed transaction coordinator
```

None of these approaches guarantee both atomicity and reliable delivery.

### Why This Matters

The Outbox Pattern emerged as the industry-standard solution to this problem, documented by Chris Richardson:

> **"The Transactional Outbox pattern uses the database as a temporary message queue. Services that send messages insert them into an outbox table as part of the database transaction that updates business entities."**  
> — Chris Richardson, Microservices Patterns (2018)

---

## Decision Drivers

1. **Guaranteed Delivery** - Integration events must not be lost, even during system failures
2. **Transactional Consistency** - Event publishing must be atomic with business state changes
3. **System Resilience** - Must survive application crashes and restarts
4. **At-Least-Once Semantics** - Events delivered at least once (idempotency handled by consumers)
5. **Retry Capability** - Failed deliveries automatically retried with backoff
6. **Microservices Readiness** - Supports evolution to distributed architecture with message bus

---

## Options Considered

### Option 1: Fire-and-Forget (Task.Run)

**Pattern:**
Execute integration event handlers in background tasks without persistence.


**Pros:**
- Simple implementation
- No additional infrastructure
- Low latency

**Cons:**
- Events lost on application crash
- No retry mechanism
- No delivery guarantees
- Cannot track processing status

**Verdict:** Rejected - No reliability guarantees

---

### Option 2: In-Memory Queue with Background Service

**Pattern:**
Queue events in memory, background service processes them.

**Pros:**
- Better than fire-and-forget
- Controlled processing
- Can implement retry logic

**Cons:**
- Queue lost on application restart
- Events in queue not persisted
- Memory constraints for high volume
- Still loses events during crashes

**Verdict:** Rejected - Not restart-safe

---

### Option 3: Transactional Outbox Pattern (Chosen)

**Pattern:**
Write integration events to a database table within the same transaction as the business operation. A separate background process reads from this outbox table and publishes events.

```
Business Transaction:
1. Update work order status
2. Write integration event to Outbox table
3. Commit transaction (both updates atomic)

Background Process:
1. Poll Outbox table for unprocessed events
2. Publish event to handlers/message bus
3. Mark event as processed
4. Retry on failure
```

**Pros:**
- Guaranteed delivery (transactional consistency)
- Survives application restarts (persisted)
- Automatic retry with configurable attempts
- At-least-once delivery semantics
- Can track processing history
- Production-proven pattern
- Supports future message bus integration

**Cons:**
- Additional database table
- Eventual consistency (small delay)
- Background processor complexity
- Storage overhead for event history

**Verdict:** Accepted - Industry-standard solution balancing reliability and complexity

---

## Decision

**Implement the Transactional Outbox Pattern with the following design:**

### Core Components

**1. Outbox Table**
Stores integration events within the same database as business entities
**2. Event Writer (DomainEventsPipeline)**
Writes integration events to outbox within the command's transaction

**3. Outbox Processor (Background Service)**
Polls outbox table and processes undelivered events:
- Runs as hosted service (BackgroundService)
- Polls at configurable interval (default: 5 seconds)
- Reads batch of unprocessed events
- Deserializes and invokes integration event handlers
- Marks events as processed or increments retry count
- Handles failures with automatic retry

**4. Outbox Store (Repository)**
Abstracts database operations for the outbox

### Separate Database Context

The outbox uses a dedicated `OutboxDbContext` separate from the main application context:

**Why Separate?**
- Independent lifecycle (outbox processor vs business transactions)
- Different connection string for potential database separation (future)
- Cleaner dependency management
- Separate migrations and schema evolution
- Enables future extraction to dedicated service

---

### Key Design Decisions

**Serialization Strategy:**
- JSON serialization with `AssemblyQualifiedName` for type resolution
- Allows version-tolerant deserialization
- Events should be immutable data contracts

**Polling vs Push:**
- Polling chosen for simplicity and reliability
- Configurable interval balances latency vs database load
- Push-based approaches (triggers, change data capture) more complex

**Retry Strategy:**
- Configurable max retries per event (default: 3)
- Failed events remain in outbox for investigation
- Future: Exponential backoff, dead letter queue

**Idempotency:**
- Handlers must be idempotent (at-least-once delivery)
- Same event may be processed multiple times on retry
- Handler should detect and ignore duplicate processing

---

## Future Evolution

### Phase 1: Default Implementation (Simple)

**Characteristics:**
- Single application instance
- In-process event handlers
- Shared database for outbox
- Simple deployment
- **EF Core-based `IOutboxStore` implementation** (`EfCoreOutboxStore`)

**Key Point:** The `IOutboxStore` abstraction makes this a simple starting point that's easy to replace later.

### Phase 2: Message Bus Integration

**Changes:**
- Outbox processor publishes to RabbitMQ/Azure Service Bus
- Handlers can be in-process or external subscribers
- Events available to external systems
- No changes to business code or handler interfaces

### Phase 3: Microservices

**Characteristics:**
- Each service has its own outbox
- Cross-service communication via message bus
- Event schema registry for contracts
- Distributed event flow

### Migration Path

**No Breaking Changes:**
- Handler interfaces remain identical (`IIntegrationEventHandler<T>`)
- Business code unchanged
- Only infrastructure swapped (in-process → message bus)
- Gradual migration possible (some events in-process, some via bus)

---

## Operational Considerations

### Monitoring

**Key Metrics:**
- Outbox processing lag (time between created and processed)
- Failed event count
- Retry count distribution
- Processing throughput

**Alerts:**
- Events exceeding max retries
- Processing lag exceeding threshold
- Outbox table growth rate

### Maintenance

**Cleanup Strategy:**
Successfully processed events should be archived/deleted:
- Keep recent history for debugging (e.g., 30 days)
- Archive older events to separate table
- Regular cleanup job prevents unbounded growth

**Dead Letter Queue (Future):**
Events exceeding max retries moved to dead letter table:
- Manual investigation required
- Can be replayed after fixing issues
- Separate from active processing

---

## Guidelines

### Writing Integration Event Handlers

**Handler Requirements:**
- Must be idempotent (same event processed multiple times has same effect)
- Should not throw exceptions for expected failures (return gracefully)
- Should log errors for investigation
- Should not access repositories (read-only operations acceptable)

### Event Design

**Best Practices:**
- Events are immutable data contracts
- Include all necessary data (avoid requiring database lookups)
- Use semantic versioning for schema changes
- Include correlation/causation IDs for tracing

---

