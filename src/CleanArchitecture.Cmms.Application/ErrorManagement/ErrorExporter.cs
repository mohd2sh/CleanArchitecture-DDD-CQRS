namespace CleanArchitecture.Cmms.Application.ErrorManagement;

using System.Reflection;
using CleanArchitecture.Cmms.Application.ErrorManagement.Models;
using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Domain.Abstractions.Attributes;

/// <summary>
/// Unified exporter for both domain and application errors.
/// Discovers errors via attributes across both layers.
/// </summary>
public class ErrorExporter : IErrorExporter
{
    /// <summary>
    /// Exports all errors (both domain and application).
    /// </summary>
    public ErrorExportResult ExportAll()
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
    public Dictionary<string, DomainErrorInfo> ExportDomainErrors()
    {
        var domainAssembly = typeof(Domain.WorkOrders.WorkOrderErrors).Assembly;
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
                .Where(f => f.FieldType == typeof(Core.Domain.Abstractions.DomainError)
                    && f.IsInitOnly  // readonly fields
                    && f.GetCustomAttribute<DomainErrorAttribute>() != null);

            foreach (var field in fields)
            {
                var domainError = (Core.Domain.Abstractions.DomainError)field.GetValue(null)!;

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
    public Dictionary<string, ApplicationErrorInfo> ExportApplicationErrors()
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
