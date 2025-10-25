# ADR-005: Attribute-Based Error Management System

**Author:** Mohammad Shakhtour  
**Status:** Accepted  

## Context

### The Problem

In enterprise applications with Clean Architecture and DDD, error handling presents significant challenges that impact both development velocity and user experience:

**Challenge 1: Error Code Fragmentation**
```csharp
// Scattered across the codebase - no single source of truth
return Error.NotFound("ASSET_NOT_FOUND", "Asset not found");
return Error.NotFound("AssetNotFound", "Asset not found");
return Error.NotFound("Asset.NotFound", "Asset not found");

//OR

public const string AssetNotFoundErrorMessage = "Asset not found";
//etc..
```

Which format is correct? How does the frontend know what error codes exist?

**Challenge 2: Frontend Localization**

Modern applications require internationalization (i18n). The frontend needs:
- Stable error codes that won't change
- Complete list of all possible errors
- Ability to map error codes to localized messages

Without a centralized error management system, this becomes manual and error-prone.

**Challenge 3: Consistency Enforcement**

How do we ensure:
- Error codes are unique across the system?
- Every error has proper structure (code + message)?
- Domain errors stay in domain, application errors in application?
- Developers follow the pattern consistently?

**Challenge 4: Discoverability**

- How does a new developer discover what errors exist?
- How do we generate API documentation with error codes?
- How do we prevent duplicate error codes?

### Why This Matters

**For Frontend Teams:**
> "We need a stable contract for error codes so we can implement proper localization and user-friendly error messages."

**For Backend Developers:**
> "We need clear guidance on where to define errors and confidence that our error codes won't conflict with others."

**For System Reliability:**
> "We need compile-time or test-time guarantees that our error handling is consistent and complete."

---

## Decision Drivers

1. **Centralized Management** - Single source of truth per aggregate/bounded context
2. **Frontend Integration** - Export API for client-side localization (i18n/l10n)
3. **Discoverability** - Easy to find all error codes via reflection
4. **Type Safety** - Strongly-typed errors, not magic strings
5. **Architecture Governance** - Automated test enforcement of patterns
6. **Layer Separation** - Domain errors vs Application errors clearly distinguished
7. **Uniqueness Guarantee** - Error codes must be unique, enforced by tests
8. **Developer Experience** - Clear, consistent pattern that's hard to misuse

---

## Decision

Implement an **Attribute-Based Error Discovery System** with centralized error definitions, with export, and architecture test enforcement.

### Core Components

#### 1. Attributes for Discovery

**`[ErrorCodeDefinition("Domain")]`** - Class-level attribute
- Marks a static class as an error container
- Specifies the domain/aggregate (e.g., "Asset", "WorkOrder")
- Used by reflection to discover error classes

**`[DomainError]`** - Field-level attribute
- Marks a `static readonly DomainError` field for export
- Indicates a domain-level business rule violation
- Enforced by architecture tests

**`[ApplicationError]`** - Field-level attribute
- Marks a `static readonly Error` field for export
- Indicates an application-level failure (NotFound, Validation, etc.)
- Enforced by architecture tests

#### 2. Error Types by Layer

**Domain Layer - `DomainError`**
```csharp
public sealed class DomainError
{
    public string Code { get; }
    public string Message { get; }
    
    public static DomainError Create(string code, string message)
        => new(code, message);
}
```
- Immutable, simple structure
- Represents business rule violations
- No error type categorization (all domain rules are equal)

**Application Layer - `Error`**
```csharp
public sealed class Error
{
    public string Code { get; }
    public string Message { get; }
    public ErrorType Type { get; }  // Validation, NotFound, Conflict, etc.
    
    public static Error NotFound(string code, string message) => ...;
    public static Error Validation(string code, string message) => ...;
    public static Error Conflict(string code, string message) => ...;
}
```
- Includes error type for HTTP status code mapping
- Represents application-level failures
- Factory methods for type safety

#### 3. Exception Types

**`DomainException`** - Thrown from Domain Layer
```csharp
public class DomainException : Exception
{
    public DomainError Error { get; }
    
    public DomainException(DomainError error) : base(error.Message)
    {
        Error = error;
    }
}
```

**`ApplicationException`** - Thrown from Application Layer
```csharp
public class ApplicationException : Exception
{
    public Error Error { get; }
    
    public ApplicationException(Error error) : base(error.Message)
    {
        Error = error;
    }
}
```

**Usage Philosophy:**
- Application layer prefers Result pattern over exceptions
- Exceptions caught and converted to Results/ProblemDetails in middleware

#### 4. Export System

**`ErrorExporter`** 
```csharp
public static class ErrorExporter
{
    public static ErrorExportResult ExportAll()
    {
        return new ErrorExportResult
        {
            DomainErrors = ExportDomainErrors(),
            ApplicationErrors = ExportApplicationErrors(),
            Timestamp = DateTime.UtcNow
        };
    }
}
```

**Process:**
1. Scan domain assembly for classes with `[ErrorCodeDefinition]`
2. Find fields with `[DomainError]` or `[ApplicationError]`
3. Extract error code, message, type, domain, class name
4. Return structured JSON for frontend consumption
5. can be cached if needed

**Export API Endpoints:**
```
GET /api/v1/errors/export          # All errors
GET /api/v1/errors/application     # Application errors only
GET /api/v1/errors/domain          # Domain errors only
```

#### 5. Architecture Test Enforcement

**Tests Automatically Enforce:**
```csharp
[Fact]
public void DomainErrorClasses_ShouldHaveErrorCodeDefinitionAttribute()
{
    // All classes ending with "Errors" must have [ErrorCodeDefinition]
}

[Fact]
public void DomainErrorFields_ShouldHaveDomainErrorAttribute()
{
    // All DomainError fields must have [DomainError]
}

[Fact]
public void ApplicationErrorFields_ShouldHaveApplicationErrorAttribute()
{
    // All Error fields must have [ApplicationError]
}

[Fact]
public void AllErrorCodes_ShouldBeUnique()
{
    // Error codes must be unique within each layer
}
```

---

## Implementation Patterns

### Domain Layer Pattern

**Domain Error Definition:**
```csharp
using CleanArchitecture.Cmms.Domain.Abstractions;
using CleanArchitecture.Cmms.Domain.Abstractions.Attributes;

namespace CleanArchitecture.Cmms.Domain.Assets;

[ErrorCodeDefinition("Asset")]
internal static class AssetErrors
{
    [DomainError]
    public static readonly DomainError AlreadyUnderMaintenance = DomainError.Create(
        "Asset.AlreadyUnderMaintenance",
        "Asset already under maintenance.");

    [DomainError]
    public static readonly DomainError NotUnderMaintenance = DomainError.Create(
        "Asset.NotUnderMaintenance",
        "Asset is not under maintenance.");

    [DomainError]
    public static readonly DomainError TagRequired = DomainError.Create(
        "AssetTag.TagRequired",
        "Asset tag cannot be empty.");
}
```

**Usage in Domain:**
```csharp
public sealed class Asset : AggregateRoot<Guid>
{
    public Result SetUnderMaintenance()
    {
        if (_status == AssetStatus.UnderMaintenance)
        {
            // Invariant violation - throw exception
            throw new DomainException(AssetErrors.AlreadyUnderMaintenance);
        }
        
        _status = AssetStatus.UnderMaintenance;
        return Result.Success();
    }
}
```

## Alternatives Considered

### Option 1: Magic Strings Everywhere

**Pattern:**
```csharp
return Error.NotFound("ASSET_NOT_FOUND", "Asset not found");
return Error.NotFound("AssetNotFound", "Asset not found");
return Error.NotFound("Asset.NotFound", "Asset not found");
```

**Analysis:**

**Problems:**
- No central source of truth
- Prone to typos and inconsistencies
- Impossible to discover all error codes
- No uniqueness guarantees
- Difficult for frontend integration

**Verdict:** Rejected - No discoverability, no consistency

---

### Option 2: Enum-Based Error Codes

**Pattern:**
```csharp
public enum ErrorCode
{
    AssetNotFound,
    AssetNotAvailable,
    WorkOrderNotFound,
    TechnicianNotAvailable
}

return Error.NotFound(ErrorCode.AssetNotFound, "Asset not found");
```

**Pros:**
- Type-safe
- Compile-time validation
- Easy to discover in IDE

**Cons:**
- No error messages attached to codes
- All error codes in single enum (violates bounded contexts)
- Difficult to export with metadata
- No layer separation (domain vs application)
- Enum serialization issues across versions

**Verdict:** Rejected - Violates bounded contexts, no message coupling

---

### Option 3: Exception-Based Control Flow

**Pattern:**
```csharp
public class AssetNotFoundException : DomainException { }
public class AssetNotAvailableException : DomainException { }
```

**Analysis:**

That's consider accepted and to have specific domain exception but still you need to define error code and message and also will require too many exceptions classes

**Problems:**
- Difficult to discover what exceptions a method might throw
- Breaking changes when adding new exceptions
- Result pattern is superior for expected failures
- Too many exceptions
- Aggregate exceptions is not straight forward to implement

**Verdict:** Rejected 

---

## Implementation Guidelines

### When to Use Domain vs Application Errors

**Use DomainError when:**
- Business rule violation (invariant)
- Thrown from aggregate roots or entities
- Should cause transaction rollback
- Example: `Asset.AlreadyUnderMaintenance`

**Use Application Error when:**
- Expected failure case (not exceptional)
- Not found, validation failure, concurrency conflict
- Returned in Result pattern
- Example: `Asset.NotFound`

### Error Code Naming Convention

**Format:** `{Aggregate}.{ErrorName}`

Examples:
- `Asset.NotFound`
- `WorkOrder.AlreadyCompleted`
- `Technician.NotAvailable`

**For nested types:** `{Aggregate}{Type}.{ErrorName}`
- `AssetTag.TagRequired`
- `TechnicianSkill.InvalidLevel`

---

## Future Considerations

### Potential Enhancements

**1. Parametrized error**
Pass values at runtime for the error template

