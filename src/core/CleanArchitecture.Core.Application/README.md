# CleanArchitecture.Core.Application

Core application layer abstractions, CQRS interfaces, and handler registration extensions for Clean Architecture.

## Purpose

This package provides CQRS abstractions (commands, queries, mediator) and dependency injection extensions for automatic handler discovery and registration.

## Installation

```bash
dotnet add package Mohd2sh.CleanArchitecture.Core.Application
```

## Usage

### Register Handlers

```csharp
services.AddApplicationHandlers(typeof(YourApplication.ServiceCollectionExtensions).Assembly);
```

### Command

```csharp
public record CreateProductCommand(string Name) : ICommand<Result<Guid>>;

public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, Result<Guid>>
{
    public Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // Implementation
    }
}
```

### Query

```csharp
public record GetProductQuery(Guid Id) : IQuery<Result<ProductDto>>;

public class GetProductQueryHandler : IQueryHandler<GetProductQuery, Result<ProductDto>>
{
    public Task<Result<ProductDto>> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        // Implementation
    }
}
```

### Domain Event Handler

```csharp
public class OrderCreatedEventHandler : IDomainEventHandler<OrderCreatedEvent>
{
    public Task Handle(OrderCreatedEvent @event, CancellationToken cancellationToken)
    {
        // Implementation
    }
}
```

### Integration Event Handler

```csharp
public class OrderCompletedEventHandler : IIntegrationEventHandler<OrderCompletedEvent>
{
    public Task Handle(OrderCompletedEvent @event, CancellationToken cancellationToken)
    {
        // Implementation
    }
}
```

## Key Components

- `ICommand<TResult>` - Command interface
- `IQuery<TResult>` - Query interface
- `IMediator` - Mediator interface for sending commands and queries
- `ICommandHandler<TCommand, TResult>` - Command handler interface
- `IQueryHandler<TQuery, TResult>` - Query handler interface
- `IDomainEventHandler<TEvent>` - Domain event handler interface
- `IIntegrationEventHandler<TEvent>` - Integration event handler interface

## Repository

GitHub: https://github.com/mohd2sh/CleanArchitecture-DDD-CQRS

## License

MIT

