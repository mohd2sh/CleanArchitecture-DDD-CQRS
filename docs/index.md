---
layout: default
title: "Clean Architecture CMMS: DDD + CQRS with Architecture Tests and Automated Boundary Enforcement"
description: "Complete .NET 8 application demonstrating Clean Architecture, Domain-Driven Design (DDD), and CQRS with automated architecture tests, integration tests, and event-driven cross-aggregate coordination."
keywords: "clean architecture, DDD, CQRS, .NET 8, domain-driven design, outbox pattern, enterprise architecture, C#, software architecture, CMMS, architecture tests, automated boundary enforcement"
author: "Mohammad Shakhtour"
---

# Clean Architecture CMMS: DDD + CQRS with Architecture Tests and Automated Boundary Enforcement

![Integration Tests](https://github.com/mohd2sh/CleanArchitecture-DDD-CQRS/actions/workflows/integration-tests.yml/badge.svg)
![Outbox Integration Tests](https://github.com/mohd2sh/CleanArchitecture-DDD-CQRS/actions/workflows/outbox-integration-tests.yml/badge.svg)
![Unit & Architecture Tests](https://github.com/mohd2sh/CleanArchitecture-DDD-CQRS/actions/workflows/dotnet-test.yml/badge.svg)
![Docker Build](https://github.com/mohd2sh/CleanArchitecture-DDD-CQRS/actions/workflows/docker-build.yml/badge.svg)

A complete .NET 8 application demonstrating Clean Architecture, Domain-Driven Design (DDD), and CQRS with automated architecture tests, integration tests, and event-driven cross-aggregate coordination. This template provides a solid foundation for building maintainable, testable, and scalable applications.

**Repository**: [CleanArchitecture-DDD-CQRS](https://github.com/mohd2sh/CleanArchitecture-DDD-CQRS) on GitHub

## Introduction

This application implements a **Computerized Maintenance Management System (CMMS)** - a domain that manages work orders, asset maintenance, and technician assignments. The CMMS domain is perfect for demonstrating DDD patterns because it has:

- **Clear business boundaries** (Work Orders, Technicians, Assets)
- **Complex business rules** (assignment constraints, status transitions)
- **Rich domain models** with encapsulated behavior
- **Real-world scenarios** that teams can relate to

### CMMS Domain Context

A CMMS system manages maintenance operations for organizations:

- **Assets** represent equipment, machinery, or facilities that require maintenance
- **Work Orders** track maintenance tasks, repairs, or inspections needed for assets
- **Technicians** are skilled workers who perform the maintenance work
- **Assignments** connect technicians to work orders based on skills and availability

The system handles the complete maintenance lifecycle: from creating work orders when issues are reported, to assigning qualified technicians, tracking progress, and completing the work.

## Philosophy

This application demonstrates that implementing .NET CQRS DDD doesn't have to be complex. It shows how to apply DDD and CQRS pragmatically in .NET - with enough structure to maintain boundaries and enable testing, but without over-engineering or speculative abstractions. The unique differentiator is **automated boundary enforcement** - architecture tests that prevent violations at compile-time.

### Design Principles

- **Domain-First Design** - Business logic lives in the domain, not in services
- **Explicit Boundaries** - Each layer has a clear purpose and dependency rules
- **Testability by Design** - Every component can be tested in isolation
- **Pragmatic CQRS** - Separate read/write models where it adds value
- **Architectural Governance** - Automated tests prevent boundary violations

## Key Features Overview

This application includes implementations of enterprise patterns:

### Core Architecture
- Clean Architecture layers with dependency inversion
- DDD tactical patterns (Aggregates, Entities, Value Objects, Domain Events)
- CQRS: EF Core for writes, flexible read sources (Dapper, read replicas, Redis, Elasticsearch)
- Repository pattern with Unit of Work
- Custom Mediator: No MediatR dependency, full control over CQRS pipeline

### Event-Driven Architecture
- **Dual Event Handlers**: `IDomainEventHandler` (transactional) + `IIntegrationEventHandler` (async)
- **Outbox Pattern**: Guaranteed event delivery with at-least-once semantics
- **Cross-aggregate coordination** via domain events (per ADR-001)

### Reliability & Consistency
- Optimistic concurrency control with SQL Server RowVersion
- Result pattern for consistent error handling
- Pipeline behaviors (Validation, Transaction, Logging, Events)
- Structured error export API for frontend localization

### Quality & Documentation
- **Architecture unit tests**: automated tests enforcing DDD/Clean Architecture
- **ADRs**: Documented architectural decisions — see [Architectural Decision Records](architectural-decisions/)
- **Unit tests** for Domain & Application layers
- **OpenAPI** with versioning
- **Integration tests**: Testcontainers-based end-to-end scenarios 

## Architecture Overview

![System Architecture Overview](https://github.com/mohd2sh/CleanArchitecture-DDD-CQRS/raw/main/docs/diagrams/Overview.png)

### CQRS Flow with Events

**Write Path (Commands):**

![Command Flow](https://github.com/mohd2sh/CleanArchitecture-DDD-CQRS/raw/main/docs/diagrams/CommandFlow.png)

Shows a write path from command through handler, domain events, transactional handlers, integration event handlers, outbox pattern, and background worker.

**Read Path (Queries):**

![Query Flow](https://github.com/mohd2sh/CleanArchitecture-DDD-CQRS/raw/main/docs/diagrams/QueryFlow.png)

Shows the flexible read path supporting multiple data sources (Read Replica, Redis Cache, Elasticsearch, Dapper) with eventual consistency.

**Command Sequence Diagram:**

![Command Sequence Diagram](https://github.com/mohd2sh/CleanArchitecture-DDD-CQRS/raw/main/docs/diagrams/CommandSequenceDiagram.svg)

## Quick Start

### Option 1: Docker Compose (Recommended)

The easiest way to run the application with all dependencies:

```bash
git clone https://github.com/mohd2sh/CleanArchitecture-DDD-CQRS.git
cd CleanArchitecture-DDD-CQRS
docker-compose up
```

**What's included:**
- SQL Server 2022 container with automatic setup
- API service
- Automatic database migrations and seeding
- Access APIs

### Option 2: Local Development

Run the API locally with your own SQL Server instance:

```bash
git clone https://github.com/mohd2sh/CleanArchitecture-DDD-CQRS.git
cd CleanArchitecture-DDD-CQRS
dotnet run --project src/CleanArchitecture.Cmms.Api
```

The application automatically creates databases, runs migrations, and seeds initial data on first run.

### Explore the API

Open Swagger UI and try the endpoints:
- Create a work order
- Assign a technician
- Complete the work order
- Observe domain events and integration events in action

## Architecture Tests

The application includes many **architecture tests** that automatically enforce DDD principles and Clean Architecture boundaries. New team members can work confidently - architectural violations are caught at automated unit tests. This is the **automated boundary enforcement** that makes this implementation unique.

### Domain Layer Protection

**Immutability & Encapsulation:**
- `ValueObjects_Should_Be_Immutable` - No public setters allowed
- `ValueObjects_Should_Be_Sealed` - Prevents inheritance and maintains invariants
- `Aggregates_Should_Have_Internal_Or_Private_Constructors` - Enforces factory methods

**Type Safety:**
- `DomainEvents_Should_Be_Sealed_And_EndWith_Event` - Naming conventions enforced
- `Domain_Types_Should_Be_Internal` - Prevents domain leakage to outer layers
- `Domain_Should_Not_Depend_On_Other_Layers` - Dependency rule enforcement

### Application Layer Boundaries

**Layer Isolation:**
- `Application_Should_Not_Depend_On_Infrastructure_Or_Api` - Clean Architecture enforcement
- `Commands_And_Queries_Should_Be_Immutable` - CQRS contracts are immutable

**Read/Write Separation:**
- `QueryHandlers_Should_Not_Use_IRepository` - Queries forbidden from using write-side repositories
- `CommandHandlers_Should_Not_Use_IReadRepository` - Commands forbidden from using read-side repositories

**Why:** Enforces CQRS separation at compile-time, prevents accidental coupling

## Architectural Decision Records

This application implements several architectural patterns based on Domain-Driven Design and Clean Architecture principles.

- **[ADR-001: Cross-Aggregate Coordination Pattern](https://github.com/mohd2sh/CleanArchitecture-DDD-CQRS/blob/main/docs/architectural-decisions/ADR-001-cross-aggregate-coordination.md)** - Domain events for coordinating operations across aggregates
- **[ADR-002: Optimistic Concurrency Control](https://github.com/mohd2sh/CleanArchitecture-DDD-CQRS/blob/main/docs/architectural-decisions/ADR-002-optimistic-concurrency-control.md)** - RowVersion pattern to prevent race conditions
- **[ADR-003: Domain Events vs Integration Events](https://github.com/mohd2sh/CleanArchitecture-DDD-CQRS/blob/main/docs/architectural-decisions/ADR-003-domain-vs-integration-events.md)** - Dual handler system for different consistency requirements
- **[ADR-004: Outbox Pattern for Guaranteed Delivery](https://github.com/mohd2sh/CleanArchitecture-DDD-CQRS/blob/main/docs/architectural-decisions/ADR-004-outbox-pattern.md)** - Transactional Outbox Pattern with background processor
- **[ADR-005: Attribute-Based Error Management System](https://github.com/mohd2sh/CleanArchitecture-DDD-CQRS/blob/main/docs/architectural-decisions/ADR-005-error-management-system.md)** - Centralized error management with attribute-based discovery
- **[ADR-006: Unobtrusive Mode for Integration Events](https://github.com/mohd2sh/CleanArchitecture-DDD-CQRS/blob/main/docs/architectural-decisions/ADR-006-unobtrusive-mode-integration-events.md)** - Message conventions for discovering integration events

These ADRs document the "why" behind architectural decisions, with implementation details visible in the codebase.

## Testing Strategy

### Unit Tests
```bash
# Run all tests
dotnet test

# Run specific test categories
dotnet test --filter "Category=Domain"
dotnet test --filter "Category=Application"
```

### Test Categories
- **Domain Tests** - Business logic and rules
- **Application Tests** - Command/Query handlers
- **Architecture Tests** - Boundary enforcement
- **Integration Tests** - End-to-end scenarios

## Project Structure

```
src/
├── core/                                  # Core Framework
│   ├── CleanArchitecture.Core.Application
│   └── CleanArchitecture.Core.Domain
│
├── CleanArchitecture.Cmms.Domain/          # Domain Layer
├── CleanArchitecture.Cmms.Application/   # Application Layer
├── CleanArchitecture.Cmms.Infrastructure/ # Infrastructure Layer
│
├── outbox/                                # Outbox Pattern
│   ├── CleanArchitecture.Outbox.Abstractions
│   └── CleanArchitecture.Outbox
│
└── CleanArchitecture.Cmms.Api/            # API Layer

tests/
├── CleanArchitecture.Cmms.Domain.UnitTests/
├── CleanArchitecture.Cmms.Application.UnitTests/
├── CleanArchitecture.Cmms.Infrastructure.UnitTests/
└── CleanArchitecture.Cmms.IntegrationTests/
```

## Contributing

We welcome contributions! Please see our [Contributing Guidelines](https://github.com/mohd2sh/CleanArchitecture-DDD-CQRS/blob/main/CONTRIBUTING.md) for details.

## License

This project is licensed under the MIT License - see the [LICENSE](https://github.com/mohd2sh/CleanArchitecture-DDD-CQRS/blob/main/LICENSE) file for details.

## Repository Links

- **GitHub Repository**: [CleanArchitecture-DDD-CQRS](https://github.com/mohd2sh/CleanArchitecture-DDD-CQRS)
- **Architectural Decision Records**: [docs/architectural-decisions/](https://github.com/mohd2sh/CleanArchitecture-DDD-CQRS/tree/main/docs/architectural-decisions)
- **Architecture Diagrams**: [docs/diagrams/](https://github.com/mohd2sh/CleanArchitecture-DDD-CQRS/tree/main/docs/diagrams)

## Give it a Star

If this application helped you or your team, please consider giving it a star on [GitHub](https://github.com/mohd2sh/CleanArchitecture-DDD-CQRS)! It helps others discover this project and motivates continued development.

---

**Built for the .NET community**
