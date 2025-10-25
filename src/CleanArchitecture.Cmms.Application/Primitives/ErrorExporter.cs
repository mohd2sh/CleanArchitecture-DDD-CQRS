namespace CleanArchitecture.Cmms.Application.Primitives;

using System.Reflection;
using CleanArchitecture.Cmms.Domain.Abstractions.Attributes;

/// <summary>
/// Unified exporter for both domain and application errors.
/// Discovers errors via attributes across both layers.
/// </summary>
public static class ErrorExporter
{
    /// <summary>
    /// Exports all errors (both domain and application).
    /// </summary>
    public static ErrorExportResult ExportAll()
    {
        return new ErrorExportResult
        {
            DomainErrors = ExportDomainErrors(),
            ApplicationErrors = ExportApplicationErrors(),
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Exports domain error objects marked with [DomainError].
    /// </summary>
    public static Dictionary<string, DomainErrorInfo> ExportDomainErrors()
    {
        var domainAssembly = typeof(Domain.Abstractions.DomainException).Assembly;
        var errors = new Dictionary<string, DomainErrorInfo>();

        // Find all classes with [ErrorCodeDefinition]
        var errorClasses = domainAssembly.GetTypes()
            .Where(t => t.IsClass
                && t.IsAbstract  // static classes are abstract
                && t.IsSealed    // static classes are sealed
                && t.GetCustomAttribute<ErrorCodeDefinitionAttribute>() != null);

        foreach (var errorClass in errorClasses)
        {
            var domainAttr = errorClass.GetCustomAttribute<ErrorCodeDefinitionAttribute>()!;

            // Get all static readonly DomainError fields with [DomainError]
            var fields = errorClass.GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.FieldType == typeof(Domain.Abstractions.DomainError)
                    && f.IsInitOnly  // readonly fields
                    && f.GetCustomAttribute<DomainErrorAttribute>() != null);

            foreach (var field in fields)
            {
                var domainError = (Domain.Abstractions.DomainError)field.GetValue(null)!;

                errors[domainError.Code] = new DomainErrorInfo
                {
                    Code = domainError.Code,
                    Message = domainError.Message,
                    Domain = domainAttr.Domain,
                    FieldName = field.Name,
                    ClassName = errorClass.Name
                };
            }
        }

        return errors;
    }

    /// <summary>
    /// Exports application Error objects marked with [ApplicationError].
    /// </summary>
    public static Dictionary<string, ApplicationErrorInfo> ExportApplicationErrors()
    {
        var applicationAssembly = typeof(ErrorExporter).Assembly;
        var errors = new Dictionary<string, ApplicationErrorInfo>();

        // Find all classes with [ErrorCodeDefinition]
        var errorClasses = applicationAssembly.GetTypes()
            .Where(t => t.IsClass
                && t.IsAbstract  // static classes are abstract
                && t.IsSealed    // static classes are sealed
                && t.GetCustomAttribute<ErrorCodeDefinitionAttribute>() != null);

        foreach (var errorClass in errorClasses)
        {
            var domainAttr = errorClass.GetCustomAttribute<ErrorCodeDefinitionAttribute>()!;

            // Get all static readonly Error fields with [ApplicationError]
            var fields = errorClass.GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.FieldType == typeof(Error)
                    && f.IsInitOnly  // readonly fields
                    && f.GetCustomAttribute<ApplicationErrorAttribute>() != null);

            foreach (var field in fields)
            {
                var error = (Error)field.GetValue(null)!;

                errors[error.Code] = new ApplicationErrorInfo
                {
                    Code = error.Code,
                    Message = error.Message,
                    Type = error.Type.ToString(),
                    Domain = domainAttr.Domain,
                    FieldName = field.Name,
                    ClassName = errorClass.Name
                };
            }
        }

        return errors;
    }
}

public record ErrorExportResult
{
    public required Dictionary<string, DomainErrorInfo> DomainErrors { get; init; }
    public required Dictionary<string, ApplicationErrorInfo> ApplicationErrors { get; init; }
    public required DateTime Timestamp { get; init; }
}

public record DomainErrorInfo
{
    public required string Code { get; init; }
    public required string Message { get; init; }
    public required string Domain { get; init; }
    public required string FieldName { get; init; }
    public required string ClassName { get; init; }
}

public record ApplicationErrorInfo
{
    public required string Code { get; init; }
    public required string Message { get; init; }
    public required string Type { get; init; }
    public required string Domain { get; init; }
    public required string FieldName { get; init; }
    public required string ClassName { get; init; }
}
