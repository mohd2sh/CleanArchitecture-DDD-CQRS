# CleanArchitecture.Outbox.Abstractions

Abstractions for the outbox pattern implementation providing interfaces for storing and publishing integration events with guaranteed delivery.

## Purpose

This package provides abstractions for implementing the transactional outbox pattern, which ensures reliable delivery of integration events in event-driven architectures. It includes interfaces for storing outbox messages within database transactions and publishing them to message buses or in-memory handlers.

## Installation

```bash
dotnet add package Mohd2sh.CleanArchitecture.Outbox.Abstractions
```

## Key Components

- `IOutboxStore` - Interface for storing outbox messages within database transactions
- `IOutboxPublisher` - Interface for publishing integration events to message buses or in-memory handlers
- `OutboxMessage` - Entity representing an integration event in the outbox table

## Dependencies

This package has no external dependencies. It provides only abstractions that can be implemented with any persistence and messaging infrastructure.

## Repository

GitHub: https://github.com/mohd2sh/CleanArchitecture-DDD-CQRS

## License

MIT

