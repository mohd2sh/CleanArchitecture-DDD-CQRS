# CleanArchitecture.Core.Domain

Core domain abstractions and base types for Clean Architecture applications following Domain-Driven Design principles.

## Purpose

This package provides base classes and interfaces for building domain models in Clean Architecture applications. It includes abstractions for entities, value objects, aggregate roots, and domain events.

## Installation

```bash
dotnet add package CleanArchitecture.Core.Domain
```

## Usage

### Entity

```csharp
public class Product : Entity<Guid>
{
    public string Name { get; private set; }
    
    private Product() { }
    
    public Product(Guid id, string name)
    {
        Id = id;
        Name = name;
    }
}
```

### Value Object

```csharp
public record Money : ValueObject
{
    public decimal Amount { get; init; }
    public string Currency { get; init; }
    
    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Amount;
        yield return Currency;
    }
}
```

### Aggregate Root

```csharp
public class Order : AggregateRoot<Guid>
{
    public string OrderNumber { get; private set; }
    
    private Order() { }
    
    public Order(Guid id, string orderNumber) : base(id)
    {
        OrderNumber = orderNumber;
    }
}
```

### Domain Event

```csharp
public sealed record OrderCreatedEvent(Guid OrderId) : IDomainEvent;
```

## Key Components

- `Entity<TId>` - Base class for domain entities
- `ValueObject` - Base class for value objects
- `AggregateRoot<TId>` - Base class for aggregate roots with optimistic concurrency
- `IDomainEvent` - Interface for domain events
- `AuditableEntity<TId>` - Base class with audit fields

## Repository

GitHub: https://github.com/mohd2sh/CleanArchitecture-DDD-CQRS

## License

MIT

