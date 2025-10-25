using System.Reflection;
using CleanArchitecture.Cmms.Application.Abstractions.Common;
using CleanArchitecture.Cmms.Application.ErrorManagement;
using CleanArchitecture.Cmms.Domain.Abstractions.Attributes;

namespace CleanArchitecture.Cmms.Application.UnitTests.ArchitectureTests;

public class ErrorManagementArchitectureTests
{

    [Fact]
    public void DomainErrorClasses_ShouldHaveErrorCodeDefinitionAttribute()
    {
        var domainAssembly = typeof(Domain.Abstractions.DomainException).Assembly;

        var errorClasses = domainAssembly.GetTypes()
            .Where(t => t.IsClass && t.Name.EndsWith("Errors"))
            .ToList();

        var classesWithoutAttribute = errorClasses
            .Where(t => t.GetCustomAttribute<ErrorCodeDefinitionAttribute>() == null)
            .ToList();

        classesWithoutAttribute.Should().BeEmpty(
            "All domain error classes should have [ErrorCodeDefinition] attribute");
    }

    [Fact]
    public void DomainErrorFields_ShouldHaveDomainErrorAttribute()
    {
        var domainAssembly = typeof(Domain.Abstractions.DomainException).Assembly;

        var errorClasses = domainAssembly.GetTypes()
            .Where(t => t.IsClass && t.Name.EndsWith("Errors"));

        var fieldsWithoutAttribute = new List<(Type Class, FieldInfo Field)>();

        foreach (var errorClass in errorClasses)
        {
            var domainErrorFields = errorClass.GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.FieldType == typeof(Domain.Abstractions.DomainError) && f.IsInitOnly);

            foreach (var field in domainErrorFields)
            {
                if (field.GetCustomAttribute<DomainErrorAttribute>() == null)
                {
                    fieldsWithoutAttribute.Add((errorClass, field));
                }
            }
        }

        fieldsWithoutAttribute.Should().BeEmpty(
            "All domain error fields should have [DomainError] attribute");
    }

    [Fact]
    public void ApplicationErrorClasses_ShouldHaveErrorCodeDefinitionAttribute()
    {
        var applicationAssembly = typeof(Error).Assembly;

        var errorClasses = applicationAssembly.GetTypes()
            .Where(t => t.IsClass && t.Name.EndsWith("Errors") && t.Namespace?.Contains("Application") == true)
            .ToList();

        var classesWithoutAttribute = errorClasses
            .Where(t => t.GetCustomAttribute<ErrorCodeDefinitionAttribute>() == null)
            .ToList();

        classesWithoutAttribute.Should().BeEmpty(
            "All application error classes should have [ErrorCodeDefinition] attribute");
    }

    [Fact]
    public void ApplicationErrorFields_ShouldHaveApplicationErrorAttribute()
    {
        var applicationAssembly = typeof(Error).Assembly;

        var errorClasses = applicationAssembly.GetTypes()
            .Where(t => t.IsClass && t.Name.EndsWith("Errors") && t.Namespace?.Contains("Application") == true);

        var fieldsWithoutAttribute = new List<(Type Class, FieldInfo Field)>();

        foreach (var errorClass in errorClasses)
        {
            var errorFields = errorClass.GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.FieldType == typeof(Error));

            foreach (var field in errorFields)
            {
                if (field.GetCustomAttribute<ApplicationErrorAttribute>() == null)
                {
                    fieldsWithoutAttribute.Add((errorClass, field));
                }
            }
        }

        fieldsWithoutAttribute.Should().BeEmpty(
            "All application Error fields should have [ApplicationError] attribute");
    }

    [Fact]
    public void AllErrorCodes_ShouldBeUnique()
    {
        var sut = new ErrorExporter();
        var exportResult = sut.ExportAll();

        // Check domain errors are unique
        var domainDuplicates = exportResult.DomainErrors.Keys
            .GroupBy(code => code)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        domainDuplicates.Should().BeEmpty(
            "All domain error codes should be unique");

        // Check application errors are unique
        var applicationDuplicates = exportResult.ApplicationErrors.Keys
            .GroupBy(code => code)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        applicationDuplicates.Should().BeEmpty(
            "All application error codes should be unique");

    }
}
