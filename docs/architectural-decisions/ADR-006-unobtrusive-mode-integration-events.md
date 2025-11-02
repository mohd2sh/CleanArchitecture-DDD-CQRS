# ADR-006: Unobtrusive Mode for Integration Events

**Author:** Mohammad Shakhtour  
**Status:** Accepted

## Context

### The Problem

Integration events must implement `IDomainEvent` to be handled. This causes issues:

- Not all System events come from domain aggregates
- External events from other systems don't have the internal marker interfaces.
- Creates unnecessary coupling between integration and domain layers

We need a way to identify integration events without marker interfaces.

---

## Decision Drivers

1. **Flexibility** - Support events from any source (domain, system, external)
2. **Extensibility** - Easy to add new event types without changing interfaces
3. **Decoupling** - Integration events shouldn't depend on domain contracts
4. **Industry Patterns** - NServiceBus and MassTransit use message conventions (unobtrusive mode - same thing)

---

## Options Considered

### Option 1: Keep IDomainEvent Constraint

Only events implementing `IDomainEvent` can be integration events.

**Pros:** Already implemented, type safety

**Cons:** Can't handle system/external events, couples layers

**Verdict:** Rejected - Too restrictive

---

### Option 2: Remove Constraint, Use Object Type

Remove constraint, use `IIntegrationEventHandler<object>`.

**Pros:** Flexible

**Cons:** Loses type safety, messy handler code

**Verdict:** Rejected - Loses type safety

---

### Option 3: Message Conventions / Unobtrusive Mode (Chosen)

Remove constraint from handler, use conventions to identify event types. Handler stays strongly-typed: `IIntegrationEventHandler<MySystemEvent>`.

**Pros:** Flexible, still type-safe, backward compatible, extensible

**Cons:** Small runtime overhead

**Verdict:** Chosen

---

## Decision

Implemented **message conventions** (also called unobtrusive mode - same concept):

1. Removed `where TEvent : IDomainEvent` constraint from `IIntegrationEventHandler<TEvent>`
2. Created `IIntegrationEventConvention` to identify event types
3. Default conventions:
   - Name ends with "Event"
   - Implements `IDomainEvent` (backward compatible)
   - In namespace ending with `.Events` or `.IntegrationEvents`
   - Its extendable 
4. Handlers always use `IIntegrationEventHandler<TEvent>` for any `TEvent` matching conventions

```csharp
// Handler - no constraint
public interface IIntegrationEventHandler<in TEvent>
{
    Task Handle(TEvent @event, CancellationToken cancellationToken = default);
}

// Usage
public class SystemStartedEvent { }  // Name ends with "Event"

internal sealed class SystemEventHandler : IIntegrationEventHandler<SystemStartedEvent>
{
    public async Task Handle(SystemStartedEvent @event, CancellationToken ct) { }
}
```

---

## Consequences

**Pros:**
- Handles system/external/domain events and future commands
- Still type-safe, backward compatible, extensible

**Cons:**
- Small runtime convention check overhead

---

## References

- ADR-003: Domain Events vs Integration Events
- ADR-004: Outbox Pattern

