# CleanArchitecture.Core.Infrastructure

Core infrastructure implementations including custom mediator, event dispatchers, and dependency injection extensions for Clean Architecture.

## Purpose

This package provides ready-to-use implementations of IMediator, IDomainEventDispatcher, and IIntegrationEventDispatcher with dependency injection extensions. It works with CleanArchitecture.Core.Application to provide a complete CQRS and event-driven architecture foundation.

## Installation

```bash
dotnet add package CleanArchitecture.Core.Infrastructure
```

## Usage

### Register Infrastructure Services

```csharp
services.AddCoreInfrastructure();
```

### Custom Configuration

```csharp
services.AddCoreInfrastructure(options =>
{
    options.RegisterConvention = true;
    // or provide custom convention
    options.CustomConvention = new CustomIntegrationEventConvention();
});
```

### Use Mediator

```csharp
public class ProductService
{
    private readonly IMediator _mediator;
    
    public ProductService(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    public async Task<Result<Guid>> CreateProduct(string name)
    {
        var command = new CreateProductCommand(name);
        return await _mediator.Send(command);
    }
}
```

### Use Event Dispatchers

```csharp
public class OrderService
{
    private readonly IDomainEventDispatcher _domainEventDispatcher;
    private readonly IIntegrationEventDispatcher _integrationEventDispatcher;
    
    public async Task CompleteOrder(Order order)
    {
        // Domain events are dispatched within transaction
        await _domainEventDispatcher.PublishAsync(new OrderCompletedEvent(order.Id));
        
        // Integration events are dispatched via outbox pattern
        await _integrationEventDispatcher.PublishAsync(new OrderCompletedIntegrationEvent(order.Id));
    }
}
```

## Key Components

- `CustomMediator` - Implementation of IMediator
- `DomainEventDispatcher` - Implementation of IDomainEventDispatcher
- `IntegrationEventDispatcher` - Implementation of IIntegrationEventDispatcher
- `DefaultIntegrationEventConvention` - Default convention for discovering integration events

## Dependencies

Requires CleanArchitecture.Core.Application and CleanArchitecture.Core.Domain.

## Repository

GitHub: https://github.com/mohd2sh/CleanArchitecture-DDD-CQRS

## License

MIT

