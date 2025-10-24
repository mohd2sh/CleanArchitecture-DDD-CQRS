# Clean Architecture DDD CQRS Template

A production-ready template demonstrating Clean Architecture, Domain-Driven Design (DDD), and CQRS principles in .NET 8. This template provides a solid foundation for building maintainable, testable, and scalable applications.

## Overview

This template implements a **Computerized Maintenance Management System (CMMS)** - a domain that manages work orders, asset maintenance, and technician assignments. The CMMS domain is perfect for demonstrating DDD patterns because it has:

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

## ⭐ Give it a Star!

If this template helped you or your team, please consider giving it a star! It helps others discover this project and motivates continued development.

[![GitHub stars](https://img.shields.io/github/stars/mohd2sh/CleanArchitecture-DDD-CQRS?style=social)](https://github.com/mohd2sh/CleanArchitecture-DDD-CQRS/stargazers)

## Philosophy

This template demonstrates that Clean Architecture doesn't have to be complex. It shows how to apply DDD and CQRS pragmatically - with enough structure to maintain boundaries and enable testing, but without over-engineering or speculative abstractions.

### Design Principles

- **Domain-First Design** - Business logic lives in the domain, not in services
- **Explicit Boundaries** - Each layer has a clear purpose and dependency rules
- **Testability by Design** - Every component can be tested in isolation
- **Pragmatic CQRS** - Separate read/write models where it adds value
- **Architectural Governance** - Automated tests prevent boundary violations

## Key Benefits

### Type-Safe Domain Modeling
- Encapsulated aggregates with business rules
- Immutable value objects preventing invalid states
- Domain events capturing meaningful business changes

### Optimized CQRS Implementation
- **Write side (Commands):** EF Core with change tracking, transactions, and optimistic concurrency
  - Uses `IRepository` with Unit of Work for consistency and ACID guarantees
  - Row-level locking and validation to prevent race conditions
  - Business rules enforced through domain aggregates
- **Read side (Queries):** Flexible data sources for performance
  - Primary: Dapper for fast SQL queries against read replicas
  - Optional: Redis for cached reads, Elasticsearch for search, or NoSQL stores
  - Eventually consistent reads acceptable (no locks, optimized for throughput)
- Clear separation preventing accidental coupling between read/write models

### Architectural Tests
- Automated boundary enforcement
- Prevents future architectural degradation
- Documents architectural decisions in code

### Pipeline Behaviors
- Dedicated pipelines for Commands and Queries with shared behaviors
- Command Pipeline: Validation → Transaction → Domain Events
- Query Pipeline: Validation → Logging (no transactions)
- Generic behaviors applicable to both pipelines
- Cross-cutting concerns in one place, easy to add new behaviors

## What's Included

### Core Architecture
- Clean Architecture layers (Domain, Application, Infrastructure, API)
- DDD tactical patterns (Aggregates, Entities, Value Objects, Domain Events)
- CQRS with separate read/write models (EF Core + Dapper)
- Repository pattern with separate read/write repositories
- Unit of Work pattern for transaction management

### Cross-Cutting Concerns
- Result pattern for consistent error handling
- MediatR pipeline behaviors (Logging, Validation, Transaction, Domain Events)
- FluentValidation integration with automatic validation
- Serilog structured logging with request/response tracking
- Global exception handling with proper HTTP status mapping

### API & Documentation
- API versioning with Swagger/OpenAPI
- Domain event publishing and handling
- Comprehensive architectural tests
- Docker support with multi-stage builds

### Quality Assurance
- Unit tests for Domain and Application layers
- Architecture tests enforcing boundaries
- Test coverage for critical business logic
- Clear project structure and naming conventions

## What's Not Included

### Deliberately Excluded for Simplicity
- **Authentication/Authorization** - Use IdentityServer, Auth0, or Azure AD
- **Message Bus/Event Bus** - Add RabbitMQ, MassTransit, or Azure Service Bus as needed
- **Outbox Pattern** - For guaranteed event delivery in distributed systems
- **Event Sourcing** - Different paradigm, add separately if needed
- **Multi-tenancy** - Add as needed per business requirements
- **Caching Layer** - Add Redis, MemoryCache, or CDN as needed
- **API Gateway** - Use Ocelot, YARP, or cloud-native solutions
- **Production Infrastructure** - Basic Dockerfile included, add K8s configs as needed
- **Frontend Application** - API-first approach, add React/Vue/Angular separately

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                        API Layer                           │
│  Controllers, Middleware, Filters, Exception Handling      │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│                    Application Layer                        │
│  Commands, Queries, Handlers, DTOs, Validation              │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│                      Domain Layer                           │
│  Aggregates, Entities, Value Objects, Domain Events        │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│                  Infrastructure Layer                       │
│  Persistence, External Services, Message Publishing         │
└─────────────────────────────────────────────────────────────┘
```

### CQRS Flow

```
┌─────────────┐    ┌──────────────┐    ┌─────────────┐
│   Command   │───▶│   Handler    │───▶│  Write DB   │
│             │    │              │    │  (EF Core)  │
└─────────────┘    └──────────────┘    └─────────────┘
                           │
                           ▼
                   ┌──────────────┐
                   │ Domain Event │
                   │  Publishing  │
                   └──────────────┘

┌─────────────┐    ┌──────────────┐    ┌─────────────┐
│    Query    │───▶│   Handler    │───▶│   Read DB   │
│             │    │              │    │  (Dapper)   │
└─────────────┘    └──────────────┘    └─────────────┘
```

### Write vs Read Consistency Models

**Write Path (Strong Consistency):**
```
┌─────────────┐    ┌──────────────┐    ┌─────────────────┐
│   Command   │───▶│   Handler    │───▶│   WriteDB       │
│             │    │  +IRepository│    │   (EF Core)     │
└─────────────┘    └──────┬───────┘    └─────────────────┘
                          │
                          │  - ACID transactions
                          │  - Optimistic concurrency
                          │  - Row-level locks
                          ▼  - Race condition protection
                   ┌──────────────┐
                   │ Domain Event │
                   │  Publishing  │
                   └──────────────┘
```

**Read Path (Eventual Consistency):**
```
┌─────────────┐    ┌──────────────┐    ┌──────────────────┐
│    Query    │───▶│   Handler    │───▶│  Read Sources    │
│             │    │              │    │                  │
└─────────────┘    └──────────────┘    │ • Read Replica   │
                                        │ • Redis Cache    │
                                        │ • Elasticsearch  │
                                        │ • NoSQL Stores   │
                                        └──────────────────┘
                                        
• No locks, optimized for throughput
• Eventually consistent (acceptable trade-off)
• Multiple data source options
• Horizontal scaling friendly
```

**Key Principles:**
- **Commands:** Use `IRepository` + `IUnitOfWork` for consistency, validation, and race condition prevention
- **Queries:** Use `IReadDbContext` (Dapper) or alternative sources for performance
- **Separation:** Write and read models never cross paths

## Key Design Decisions

### 1. Internal Domain Types
```csharp
internal sealed class WorkOrder : AggregateRoot<Guid>
```
**Why:** Prevents domain types from leaking to other layers, enforcing encapsulation.

### 2. Separate DbContexts
```csharp
WriteDbContext (EF Core) + ReadDbContext (Dapper)
```
**Why:** Write side needs change tracking, read side needs performance optimization.

### 3. Result Pattern Over Exceptions
```csharp
return Result.Failure("Asset not found");
```
**Why:** Business rule violations aren't exceptional - they're expected business outcomes.

### 4. MediatR Abstraction
```csharp
public interface IMediator { ... }
```
**Why:** Not directly dependent on MediatR - can swap implementations if needed.

### 5. Pipeline Behaviors - Separate Command and Query Pipelines
**Command Pipeline:**
```csharp
Logging → Validation → Transaction → DomainEvents
```
- Transactions ensure atomicity of write operations
- Domain events published within transaction scope

**Query Pipeline:**
```csharp
Logging → Validation
```
- No transactions (read-only, no side effects)
- Can read from eventually consistent sources (replicas, cache, Elasticsearch)

**Generic Behaviors:**
- Logging applies to both commands and queries
- Validation enforced on both pipelines
- Custom behaviors can target specific pipeline types

**Why:** Commands require consistency and transactions; queries prioritize performance and can tolerate eventual consistency.

### 6. Domain Events Cleared After Publishing
```csharp
aggregates.ForEach(a => a.ClearDomainEvents());
```
**Why:** Prevents re-processing events, ensures clean state.

### 7. Architecture Tests
```csharp
[Fact] public void Application_Should_Not_Depend_On_Infrastructure()
```
**Why:** Automated enforcement prevents architectural degradation over time.

### 8. Bounded Contexts
```
WorkOrders/ | Technicians/ | Assets/
```
**Why:** Features isolated, preventing cross-domain coupling.

### 9. Simple Domain Exceptions
```csharp
throw new DomainException("Work order title cannot be empty");
```
**Why:** Simplicity over complex exception hierarchies. Can be enhanced to specific domain exceptions or business rules as needed.

## Error Management

### Structured Error Handling

The template implements a comprehensive error management system using attributes for discoverability, export, and architecture testing.

### Error Organization

**Domain Layer:**
```csharp
[ErrorCodeDefinition("WorkOrder")]
internal static class WorkOrderErrors
{
    [DomainError("WorkOrder.TitleRequired")]
    public const string TitleRequired = "Work order title cannot be empty.";
}
```

**Application Layer:**
```csharp
[ErrorCodeDefinition("WorkOrder")]
public static class WorkOrderErrors
{
    [ApplicationError]
    public static readonly Error NotFound = Error.NotFound(
        "WorkOrder.NotFound",
        "Work order not found.");
}
```

### Key Features

1. **Attribute-Based Discovery** - Errors marked with `[DomainError]` and `[ApplicationError]` attributes
2. **Centralized Management** - One error class per aggregate
3. **Architecture Tests** - Automated enforcement of attribute usage
4. **Export Capability** - API endpoint exports all errors for frontend localization
5. **Type Safety** - Compile-time error code validation

### Export API

```bash
# Export all errors for frontend localization
GET /api/v1/errors/export

# Export only application errors
GET /api/v1/errors/application

# Export only domain errors  
GET /api/v1/errors/domain
```

### Frontend Localization

Error codes are stable identifiers for client-side localization:

```json
{
  "domainErrors": {
    "WorkOrder.TitleRequired": {
      "code": "WorkOrder.TitleRequired",
      "message": "Work order title cannot be empty.",
      "domain": "WorkOrder"
    }
  },
  "applicationErrors": {
    "WorkOrder.NotFound": {
      "code": "WorkOrder.NotFound", 
      "message": "Work order not found.",
      "type": "NotFound",
      "domain": "WorkOrder"
    }
  }
}
```

### Architecture Tests

```csharp
[Fact]
public void DomainErrorConstants_ShouldHaveDomainErrorAttribute()
{
    // Ensures all domain error constants have [DomainError] attribute
}

[Fact] 
public void AllErrorCodes_ShouldBeUnique()
{
    // Prevents duplicate error codes across layers
}
```

## Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server (LocalDB, Express, or full)
- Visual Studio 2022 or VS Code

### Quick Start

1. **Clone the repository**
   ```bash
   git clone https://github.com/your-username/Company.Cmms.git
   cd Company.Cmms
   ```

2. **Update connection strings**
   ```json
   // appsettings.Development.json
   {
     "ConnectionStrings": {
       "WriteDb": "Server=(localdb)\\mssqllocaldb;Database=CmmsWrite;Trusted_Connection=true;",
       "ReadDb": "Server=(localdb)\\mssqllocaldb;Database=CmmsRead;Trusted_Connection=true;"
     }
   }
   ```

3. **Run the application**
   ```bash
   dotnet run --project src/CleanArchitecture.Cmms.Api
   ```

4. **Explore the API**
   - Open https://localhost:7000/swagger
   - Try creating a work order
   - Assign a technician
   - Complete the work order

### Database Setup
The application will automatically:
- Create databases if they don't exist
- Run migrations
- Seed initial data

## Project Structure

```
src/
├── CleanArchitecture.Cmms.Domain/          # Domain Layer
│   ├── Abstractions/                      # Base classes and interfaces
│   ├── WorkOrders/                        # Work Order aggregate
│   ├── Technicians/                       # Technician aggregate
│   └── Assets/                           # Asset aggregate
│
├── CleanArchitecture.Cmms.Application/     # Application Layer
│   ├── Abstractions/                      # Interfaces and contracts
│   ├── WorkOrders/                        # Work Order use cases
│   │   ├── Commands/                      # Write operations
│   │   ├── Queries/                       # Read operations
│   │   └── Dtos/                         # Data transfer objects
│   ├── Behaviors/                        # Pipeline behaviors
│   └── Primitives/                       # Result, Pagination, etc.
│
├── CleanArchitecture.Cmms.Infrastructure/ # Infrastructure Layer
│   ├── Persistence/                       # Database contexts
│   ├── Repositories/                      # Repository implementations
│   └── Messaging/                        # MediatR adapter
│
└── CleanArchitecture.Cmms.Api/           # API Layer
    ├── Controllers/                       # API endpoints
    ├── Middlewares/                      # Exception handling
    └── Filters/                          # Result mapping
```

## Testing Strategy

### Unit Tests
```bash
# Run all tests
dotnet test

# Run specific test categories
dotnet test --filter "Category=Domain"
dotnet test --filter "Category=Application"
```

### Architecture Tests
```bash
# Run architectural tests
dotnet test tests/CleanArchitecture.Cmms.Application.UnitTests/
dotnet test tests/CleanArchitecture.Cmms.Domain.UnitTests/
```

### Test Categories
- **Domain Tests** - Business logic and rules
- **Application Tests** - Command/Query handlers
- **Architecture Tests** - Boundary enforcement
- **Integration Tests** - End-to-end scenarios

## Extending the Template

### Adding a New Aggregate

1. **Create Domain Model**
   ```csharp
   // Domain/Equipment/Equipment.cs
   internal sealed class Equipment : AggregateRoot<Guid>
   {
       // Business logic here
   }
   ```

2. **Add Application Use Cases**
   ```csharp
   // Application/Equipment/Commands/CreateEquipment/
   public sealed record CreateEquipmentCommand(...) : ICommand<Result<Guid>>;
   ```

3. **Implement Infrastructure**
   ```csharp
   // Infrastructure/Persistence/EfCore/EquipmentConfiguration.cs
   public class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
   ```

4. **Add API Endpoints**
   ```csharp
   // Api/Controllers/V1/EquipmentController.cs
   [ApiController]
   public sealed class EquipmentController : ControllerBase
   ```

### Adding Custom Pipeline Behaviors

```csharp
public class CustomPipeline<TRequest, TResult> : IPipeline<TRequest, TResult>
{
    public async Task<TResult> Handle(TRequest request, RequestHandlerDelegate<TResult> next, CancellationToken cancellationToken)
    {
        // Custom logic before
        var result = await next();
        // Custom logic after
        return result;
    }
}
```

### Integration with External Systems

```csharp
// Infrastructure/ExternalServices/
public interface IEmailService
{
    Task SendAsync(string to, string subject, string body);
}

// Application/WorkOrders/EventsHandlers/
internal class WorkOrderCompletedEventHandler : INotificationHandler<WorkOrderCompletedEvent>
{
    private readonly IEmailService _emailService;
    
    public async Task Handle(WorkOrderCompletedEvent notification, CancellationToken cancellationToken)
    {
        await _emailService.SendAsync(/* notification details */);
    }
}
```

## Optimistic Concurrency Control

This template implements **optimistic concurrency control** using SQL Server's `ROWVERSION` (timestamp) columns to prevent race conditions and ensure data consistency.

### How It Works

**RowVersion Implementation:**
```csharp
// Domain/Abstractions/AggregateRoot.cs
public abstract class AggregateRoot<TId> : AuditableEntity<TId>, IAggregateRoot
{
    [Timestamp]
    public byte[] RowVersion { get; protected set; } = default!;
}

// Infrastructure/Persistence/Configurations/AuditableEntityConfiguration.cs
builder.Property(e => e.RowVersion)
    .IsRowVersion()
    .IsConcurrencyToken();
```

**Concurrency Protection Flow:**
1. **Load Entity** → RowVersion = `0x0000000000000001`
2. **Modify Entity** → Changes tracked in memory
3. **SaveChanges()** → SQL: `WHERE Id = @id AND RowVersion = @version`
4. **If RowVersion changed** → `DbUpdateConcurrencyException` thrown
5. **Exception Handling** → HTTP 409 Conflict with user-friendly error

### Race Condition Prevention

**Scenario: Concurrent Work Order Creation**
```
Time 1: Thread A loads Asset (RowVersion = 1)
Time 2: Thread B loads same Asset (RowVersion = 1)  
Time 3: Thread A creates WorkOrder → Asset.SetUnderMaintenance()
Time 4: Thread B creates WorkOrder → Asset.SetUnderMaintenance()
Time 5: Thread A commits → Asset.RowVersion becomes 2 
Time 6: Thread B commits → WHERE RowVersion = 1 → No rows affected
```

**Result:**
- Thread A succeeds: WorkOrder created, Asset under maintenance
- Thread B fails: `DbUpdateConcurrencyException` → HTTP 409 Conflict

### Error Handling

**Concurrency Exceptions:**
```csharp
// Api/Middlewares/ExceptionHandlingMiddleware.cs
case DbUpdateConcurrencyException concurrencyEx:
    await WriteConcurrencyResponseAsync(context, concurrencyEx);
    break;

// Returns HTTP 409 with:
{
    "isSuccess": false,
    "error": {
        "code": "Asset.ConcurrencyConflict",
        "message": "Asset was modified by another user. Please refresh and try again."
    }
}
```
## Architectural Decisions

This template implements several architectural patterns based on Domain-Driven Design and Clean Architecture principles. Key decisions are documented in ADRs (Architectural Decision Records):

- **[ADR-001: Cross-Aggregate Coordination Pattern](docs/architectural-decisions/ADR-001-cross-aggregate-coordination.md)** - How we handle operations that span multiple aggregates, with analysis of all DDD patterns (events, services, orchestration) and real quotes from Eric Evans and Vaughn Vernon.
- **[ADR-002: Optimistic Concurrency Control](docs/architectural-decisions/ADR-002-optimistic-concurrency-control.md)** - Implementation of RowVersion pattern to prevent race conditions and ensure data consistency in concurrent scenarios.
- **[ADR-003: Domain Events vs Integration Events](docs/architectural-decisions/ADR-003-domain-vs-integration-events.md)** - Distinction between synchronous transactional events (IDomainEventHandler) and asynchronous integration events (IIntegrationEventHandler) for different consistency requirements.
- **[ADR-004: Outbox Pattern for Guaranteed Delivery](docs/architectural-decisions/ADR-004-outbox-pattern.md)** - Implementation of the Transactional Outbox Pattern to ensure reliable, guaranteed delivery of integration events with at-least-once semantics.

## Contributing

We welcome contributions! Please see our [Contributing Guidelines](CONTRIBUTING.md) for details.

### Development Setup
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Ensure all tests pass
6. Submit a pull request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html) by Robert C. Martin
- [Domain-Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html) by Eric Evans
- [CQRS](https://martinfowler.com/bliki/CQRS.html) by Greg Young
- [MediatR](https://github.com/jbogard/MediatR) by Jimmy Bogard
- [NetArchTest](https://github.com/BenMorris/NetArchTest) by Ben Morris

---

**Built for the .NET community**