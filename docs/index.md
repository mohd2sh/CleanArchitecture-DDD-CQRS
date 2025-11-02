---
layout: default
title: "Clean Architecture DDD CQRS Template for .NET 8"
description: "Production-ready template demonstrating Clean Architecture, Domain-Driven Design (DDD), and CQRS principles in .NET 8. Complete with outbox pattern, architecture tests, and ADRs."
keywords: "clean architecture, DDD, CQRS, .NET 8, domain-driven design, outbox pattern, enterprise architecture, C#, software architecture, CMMS"
author: "Mohammad Shakhtour"
---

# Clean Architecture DDD CQRS Template for .NET 8

A template demonstrating Clean Architecture, Domain-Driven Design (DDD), and CQRS principles in .NET 8. This template provides a solid foundation for building maintainable, testable, and scalable enterprise applications.

## Overview

This repository contains implementation of enterprise software architecture patterns using a realistic Computerized Maintenance Management System (CMMS) domain. Unlike simple Todo app examples, this template demonstrates how to handle real-world complexity with proper architectural patterns.

The template is designed for teams who need a pragmatic approach to Clean Architecture - one that balances best practices with practical implementation concerns. Every architectural decision is documented through Architectural Decision Records (ADRs), explaining not just what was built, but why.

## Key Features

### Clean Architecture Implementation

The template implements strict layer separation following Clean Architecture principles:

- **Domain Layer**: Core business logic with no external dependencies
- **Application Layer**: Use cases and orchestration, depends only on domain
- **Infrastructure Layer**: External concerns like databases and messaging
- **API Layer**: Presentation layer that depends on application

Architecture tests automatically enforce these boundaries, preventing architectural degradation over time.

### Domain-Driven Design Patterns

The template demonstrates DDD tactical patterns:

- **Aggregates**: Encapsulate business logic and maintain invariants
- **Value Objects**: Ensure immutability and domain constraints
- **Domain Events**: Coordinate across aggregates without breaking boundaries
- **Bounded Contexts**: Separate feature modules (WorkOrders, Technicians, Assets)

### CQRS Architecture

Command Query Responsibility Segregation is implemented with clear separation:

- **Write Side**: Uses EF Core with change tracking for strong consistency
- **Read Side**: Uses Dapper for optimized queries and eventual consistency
- **Architecture Enforcement**: Tests prevent commands from using read repositories and queries from using write repositories

### Event-Driven Architecture

Dual event handler system provides flexibility for different consistency requirements:

- **Domain Event Handlers**: Execute synchronously within transactions for immediate consistency
- **Integration Event Handlers**: Execute asynchronously via Outbox Pattern for eventual consistency

This separation makes it explicit when you need ACID guarantees versus when eventual consistency is acceptable.

### Outbox Pattern

Complete implementation of the Transactional Outbox Pattern:

- Integration events written to database within command transaction
- Background processor handles delivery with retry logic
- Dead letter queue for failed events
- Survives application restarts
- At-least-once delivery semantics

The implementation is ready to evolve from in-process handlers to message bus integration (RabbitMQ, Azure Service Bus) without changing the core abstraction.

### Architecture Tests

Automated tests enforce architectural rules:

- Layer dependency rules (Application cannot depend on Infrastructure)
- CQRS boundary enforcement (Queries cannot use write repositories)
- Aggregate encapsulation (Internal domain types, proper constructors)
- Value object immutability
- Error management structure

These tests run in CI/CD and catch violations before code review.

### Error Management System

Structured error management with attribute-based discovery:

- Domain errors and application errors clearly separated
- Export API for frontend localization
- Stable error codes across versions
- Architecture tests ensure proper usage

## Architecture Diagrams

Visual representations of the system architecture:

- [System Overview](diagrams/Overview.png) - High-level architecture
- [Command Flow](diagrams/CommandFlow.png) - Write path with events
- [Query Flow](diagrams/QueryFlow.png) - Read path with multiple sources
- [Command Sequence](diagrams/CommandSequenceDiagram.svg) - Detailed sequence diagram

## Architectural Decision Records

Six documented architectural decisions explain the reasoning behind key choices:

1. **[ADR-001: Cross-Aggregate Coordination](architectural-decisions/ADR-001-cross-aggregate-coordination.md)** - Why domain events for coordination
2. **[ADR-002: Optimistic Concurrency Control](architectural-decisions/ADR-002-optimistic-concurrency-control.md)** - RowVersion pattern implementation
3. **[ADR-003: Domain Events vs Integration Events](architectural-decisions/ADR-003-domain-vs-integration-events.md)** - When to use each type
4. **[ADR-004: Outbox Pattern](architectural-decisions/ADR-004-outbox-pattern.md)** - Guaranteed delivery implementation
5. **[ADR-005: Error Management System](architectural-decisions/ADR-005-error-management-system.md)** - Attribute-based error handling
6. **[ADR-006: Unobtrusive Mode](architectural-decisions/ADR-006-unobtrusive-mode-integration-events.md)** - Message conventions for events

Each ADR documents the problem, considered options, decision, and consequences.

## Quick Start

### Using Docker Compose

The fastest way to get started:

```bash
git clone https://github.com/mohd2sh/CleanArchitecture-DDD-CQRS.git
cd CleanArchitecture-DDD-CQRS
docker-compose up
```

This sets up SQL Server, runs migrations, and starts the API automatically.

### Local Development

For local development:

```bash
git clone https://github.com/mohd2sh/CleanArchitecture-DDD-CQRS.git
cd CleanArchitecture-DDD-CQRS
dotnet run --project src/CleanArchitecture.Cmms.Api
```

The application automatically creates the database and runs migrations on first startup.

### Explore the API

Once running, access Swagger UI at `http://localhost:5000/swagger` to:

- Create work orders
- Assign technicians
- Complete work orders
- Observe domain events and integration events in action

## Testing

The template includes comprehensive test coverage:

- **Unit Tests**: Domain logic and application handlers
- **Architecture Tests**: Enforce architectural boundaries
- **Integration Tests**: End-to-end scenarios with Testcontainers

Run all tests:
```bash
dotnet test
```

Run architecture tests only:
```bash
dotnet test --filter "Category=Architecture"
```

## Domain Example: CMMS System

The template uses a Computerized Maintenance Management System as the domain example. This domain was chosen because it:

- Has clear business boundaries (Work Orders, Technicians, Assets)
- Requires complex business rules (assignment constraints, status transitions)
- Needs rich domain models with encapsulated behavior
- Demonstrates real-world scenarios teams encounter

### Domain Entities

**Assets**: Equipment or machinery requiring maintenance. Assets have maintenance schedules and operational status.

**Work Orders**: Maintenance tasks, repairs, or inspections for assets. Work orders track priority, status, and assignment.

**Technicians**: Skilled workers who perform maintenance. Technicians have skill sets and capacity constraints.

**Assignments**: Connect technicians to work orders based on skills and availability.

This domain complexity requires proper DDD patterns, making it a better learning example than trivial applications.

## What Makes This Different

Most architecture templates are either too simple (Todo apps that don't need DDD) or too complex (over-engineered with unnecessary abstractions). This template:

- Uses a realistic domain that actually requires the patterns demonstrated
- Includes complete implementations, not just placeholders
- Documents decisions through ADRs
- Enforces architecture through automated tests
- Balances best practices with pragmatism

## Use Cases

This template is valuable for:

- **New Projects**: Skip the architecture debate and start with a solid foundation
- **Learning DDD/CQRS**: See patterns applied to real complexity
- **Team Onboarding**: Architecture tests teach constraints automatically
- **Pattern Evaluation**: Understand how different patterns work together

## Technology Stack

- **.NET 8**: Latest framework features
- **EF Core**: Write-side data access with change tracking
- **Dapper**: Read-side optimized queries
- **SQL Server**: Database with RowVersion for concurrency
- **Testcontainers**: Integration testing with real databases
- **xUnit**: Testing framework
- **FluentValidation**: Input validation

## Contributing

Contributions are welcome. Please see the [Contributing Guidelines](https://github.com/mohd2sh/CleanArchitecture-DDD-CQRS/blob/main/CONTRIBUTING.md) for details.

## License

This project is licensed under the MIT License - see the [LICENSE](https://github.com/mohd2sh/CleanArchitecture-DDD-CQRS/blob/main/LICENSE) file for details.

## Repository

**GitHub**: [CleanArchitecture-DDD-CQRS](https://github.com/mohd2sh/CleanArchitecture-DDD-CQRS)

If you find this template useful, please give it a star on GitHub. It helps others discover better approaches to building .NET applications.

---

**Built for developers, by developers. Practical patterns for real-world applications.**

