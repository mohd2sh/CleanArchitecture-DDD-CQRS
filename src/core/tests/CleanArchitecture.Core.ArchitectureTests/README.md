# CleanArchitecture.Core.ArchitectureTests

Reusable architecture test base classes for Clean Architecture and DDD validation.

## Purpose

This package provides base test classes that consumers can inherit to enforce architectural rules and boundaries. It includes pre-built tests for domain and application layer validation following Clean Architecture and DDD principles.

## Installation

```bash
dotnet add package CleanArchitecture.Core.ArchitectureTests
```

## Usage

### Domain Architecture Tests

```csharp
using CleanArchitecture.Core.ArchitectureTests.Domain;

public class DomainArchitectureTests : DomainArchitectureTestBase
{
    protected override Assembly DomainAssembly => typeof(YourDomain.Entity).Assembly;
    
    protected override Assembly[] ForbiddenAssemblies => new[]
    {
        typeof(YourApplication.ServiceCollectionExtensions).Assembly,
        typeof(YourInfrastructure.ServiceCollectionExtensions).Assembly
    };
}
```

### Application Architecture Tests

```csharp
using CleanArchitecture.Core.ArchitectureTests.Application;

public class ApplicationArchitectureTests : ApplicationArchitectureTestBase
{
    protected override Assembly ApplicationAssembly => typeof(YourApplication.ServiceCollectionExtensions).Assembly;
    protected override Assembly DomainAssembly => typeof(YourDomain.Entity).Assembly;
    
    protected override Assembly[] ForbiddenAssemblies => new[]
    {
        typeof(YourInfrastructure.ServiceCollectionExtensions).Assembly
    };
}
```

## Tests Included

### Domain Tests

- Value objects immutability
- Domain events naming and structure
- Entities encapsulation
- Domain layer isolation

### Application Tests

- Layer dependency rules
- CQRS separation (read/write repositories)
- Handler naming conventions
- Bounded context isolation

## Dependencies

Requires xunit and NetArchTest.Rules.

## Repository

GitHub: https://github.com/mohd2sh/CleanArchitecture-DDD-CQRS

## License

MIT

