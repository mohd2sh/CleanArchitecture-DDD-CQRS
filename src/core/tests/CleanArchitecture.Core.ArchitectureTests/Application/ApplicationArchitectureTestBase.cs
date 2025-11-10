using System.Reflection;
using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Application.Abstractions.Messaging;
using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;
using CleanArchitecture.Core.ArchitectureTests.Common;
using CleanArchitecture.Core.Domain.Abstractions;
using NetArchTest.Rules;
using Xunit;

namespace CleanArchitecture.Core.ArchitectureTests.Application;

/// <summary>
/// Base class for application layer architecture tests.
/// Inherit from this class and provide the application assembly and related assemblies.
/// </summary>
/// <example>
/// <code>
/// public class ApplicationArchitectureTests : ApplicationArchitectureTestBase
/// {
///     protected override Assembly ApplicationAssembly => typeof(YourApplication.ServiceCollectionExtensions).Assembly;
///     protected override Assembly DomainAssembly => typeof(YourDomain.AggregateRoot).Assembly;
///     protected override Assembly[] ForbiddenAssemblies => new[]
///     {
///         typeof(YourInfrastructure.ServiceCollectionExtensions).Assembly
///     };
/// }
/// </code>
/// </example>
public abstract class ApplicationArchitectureTestBase
{
    /// <summary>
    /// The application assembly to test. Must be provided by the consumer.
    /// </summary>
    protected abstract Assembly ApplicationAssembly { get; }

    /// <summary>
    /// The domain assembly (for bounded context discovery). Must be provided by the consumer.
    /// </summary>
    protected abstract Assembly DomainAssembly { get; }

    /// <summary>
    /// Assemblies that the application should NOT depend on (e.g., Infrastructure, API).
    /// </summary>
    protected abstract Assembly[] ForbiddenAssemblies { get; }

    [Fact]
    public void Application_Should_Not_Depend_On_Infrastructure_Or_Api()
    {
        // Arrange
        var forbiddenAssemblyNames = ForbiddenAssemblies
            .Select(a => a.GetName().Name!)
            .ToArray();

        // Act
        var result = Types
            .InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenAssemblyNames)
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"Application layer must not depend on: {string.Join(", ", forbiddenAssemblyNames)}");
    }

    [Fact]
    public void Commands_And_Queries_Should_Be_Immutable()
    {
        // Arrange
        var cqrsTypes = Types
            .InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(ICommand<>))
            .Or()
            .ImplementInterface(typeof(IQuery<>))
            .GetTypes();

        // Act
        var invalid = cqrsTypes
            .SelectMany(t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            .Where(p =>
            {
                if (p.SetMethod == null) return false;
                if (!p.SetMethod.IsPublic) return false;

                // Allow init-only setters
                return !ArchitectureTestHelpers.IsInitOnlySetter(p);
            })
            .ToList();

        // Assert
        Assert.True(!invalid.Any(),
            $"Commands/Queries must be immutable. Mutable: {string.Join(", ", invalid.Select(p => p.DeclaringType?.Name + '.' + p.Name))}");
    }

    [Fact]
    public void Command_And_Query_Handlers_Should_EndWith_Handler()
    {
        // Arrange
        var handlers = Types
            .InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(ICommandHandler<,>))
            .Or()
            .ImplementInterface(typeof(IQueryHandler<,>))
            .GetTypes();

        // Act
        var invalid = handlers
            .Where(t => !t.Name.EndsWith("Handler", StringComparison.Ordinal))
            .ToList();

        // Assert
        Assert.True(!invalid.Any(),
            $"All handlers should end with 'Handler'. Found: {string.Join(", ", invalid.Select(t => t.Name))}");
    }

    [Fact]
    public void QueryHandlers_Should_Not_Use_IRepository()
    {
        // Arrange
        var writeRepoType = typeof(IRepository<,>);
        var queryHandlers = Types
            .InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(IQueryHandler<,>))
            .GetTypes();

        // Act
        var invalid = queryHandlers
            .SelectMany(t => t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            .Where(f => f.FieldType.IsGenericType &&
                        f.FieldType.GetGenericTypeDefinition() == writeRepoType)
            .Select(f => f.DeclaringType)
            .Distinct()
            .ToList();

        // Assert
        Assert.True(!invalid.Any(),
            $"Query handlers should not depend on IRepository (write-side). Found: {string.Join(", ", invalid.Select(t => t?.Name))}");
    }

    [Fact]
    public void CommandHandlers_Should_Not_Use_IReadRepository()
    {
        // Arrange
        var readRepoType = typeof(IReadRepository<,>);
        var commandHandlers = Types
            .InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(ICommandHandler<,>))
            .GetTypes();

        // Act
        var invalid = commandHandlers
            .SelectMany(t => t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            .Where(f => f.FieldType.IsGenericType &&
                        f.FieldType.GetGenericTypeDefinition() == readRepoType)
            .Select(f => f.DeclaringType)
            .Distinct()
            .ToList();

        // Assert
        Assert.True(!invalid.Any(),
            $"Command handlers should not depend on IReadRepository (read-side). Found: {string.Join(", ", invalid.Select(t => t?.Name))}");
    }

    [Fact]
    public void Handlers_Should_Not_Return_Domain_Types()
    {
        // Arrange
        var domainTypes = DomainAssembly.GetTypes()
            .Where(t => t.IsAssignableTo(typeof(ValueObject)) ||
                        t.IsAssignableTo(typeof(IAggregateRoot)) ||
                        t.BaseType != null && t.BaseType.IsGenericType &&
                         t.BaseType.GetGenericTypeDefinition() == typeof(IEntity<>))
            .Select(t => t.FullName)
            .Where(name => name != null)
            .ToHashSet();

        var handlers = Types
            .InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(ICommandHandler<,>))
            .Or()
            .ImplementInterface(typeof(IQueryHandler<,>))
            .GetTypes();

        // Act
        var invalid = handlers
            .Where(h =>
            {
                var handleMethod = h.GetMethod("Handle");
                if (handleMethod == null) return false;
                var returnType = handleMethod.ReturnType;
                return returnType.FullName != null && domainTypes.Contains(returnType.FullName);
            })
            .ToList();

        // Assert
        Assert.True(!invalid.Any(),
            $"Handlers should not return domain entities or value objects. Found: {string.Join(", ", invalid.Select(t => t.Name))}");
    }

    [Fact]
    public void Command_And_Query_Handlers_Should_Not_Be_Public()
    {
        // Arrange
        var handlers = Types
            .InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(ICommandHandler<,>))
            .Or()
            .ImplementInterface(typeof(IQueryHandler<,>))
            .GetTypes();

        // Act
        var invalid = handlers.Where(t => t.IsPublic).ToList();

        // Assert
        Assert.True(!invalid.Any(),
            $"Handlers should be internal, not public. Found: {string.Join(", ", invalid.Select(t => t.Name))}");
    }

    [Fact]
    public void Application_Features_Should_Be_BoundedContexts()
    {
        // Arrange
        // Dynamically resolve base namespaces (strip ".Domain" or ".Application")
        var domainBaseNamespace = typeof(IAggregateRoot).Namespace!.Split('.')[..^1]; // remove ".Abstractions"
        var baseNamespace = string.Join(".", domainBaseNamespace);

        // Discover feature namespaces dynamically from aggregate roots
        var featureNamespaces = DomainAssembly.GetTypes()
            .Where(t => typeof(IAggregateRoot).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
            .Select(t => t.Namespace)
            .Where(ns => ns != null && ns!.Contains('.'))
            .Select(ns => ns!.Split('.').SkipWhile(p => !p.Equals("Domain")).Skip(1).FirstOrDefault())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .OrderBy(x => x)
            .ToArray();

        // Act & Assert per feature
        foreach (var feature in featureNamespaces)
        {
            var appFeatureNamespace = $"{baseNamespace}.Application.{feature}";

            var otherAppNamespaces = featureNamespaces
                .Where(other => other != feature)
                .Select(other => $"{baseNamespace}.Application.{other}")
                .ToArray();

            var result = Types
                .InAssembly(ApplicationAssembly)
                .That()
                .ResideInNamespace(appFeatureNamespace)
                .ShouldNot()
                .HaveDependencyOnAny(otherAppNamespaces)
                .GetResult();

            Assert.True(result.IsSuccessful,
                $"Feature '{feature}' should not depend on other application features.");
        }
    }

    [Fact]
    public void All_Commands_And_Queries_Should_Return_ResultTypeResponse()
    {
        var types = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(ICommand<>))
            .Or()
            .ImplementInterface(typeof(IQuery<>))
            .GetTypes();

        var invalid = new List<string>();

        foreach (var type in types)
        {
            var interfaceType = type.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>));

            var returnType = interfaceType?.GetGenericArguments().FirstOrDefault();
            if (returnType == null)
                continue;

            var isResult = returnType == typeof(Result)
                         || returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Result<>);

            if (!isResult)
                invalid.Add($"{type.Name} → returns {returnType.Name}");
        }

        Assert.True(invalid.Count == 0,
            $"All CQRS requests should return Result or Result<T>. Invalid: {string.Join(", ", invalid)}");
    }

    [Fact]
    public void CommandHandlers_Should_Use_Repository_From_Same_BoundedContext()
    {
        var handlers = Types
            .InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(ICommandHandler<,>))
            .GetTypes();

        var violations = new List<string>();

        foreach (var handler in handlers.Where(t => !t.IsAbstract))
        {
            var handlerContext = ArchitectureTestHelpers.ExtractBoundedContext(handler.Namespace, "Application");

            // Collect injected IRepository<T,> types (via fields or ctor)
            var ctorParamTypes = handler
                .GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .SelectMany(c => c.GetParameters().Select(p => p.ParameterType));

            var fieldTypes = handler
                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(f => f.FieldType);

            var repoTypes = ctorParamTypes.Concat(fieldTypes)
                .Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IRepository<,>))
                .Select(t => t.GetGenericArguments()[0]) // the aggregate type
                .Distinct();

            foreach (var aggregateType in repoTypes)
            {
                var aggregateContext = ArchitectureTestHelpers.ExtractBoundedContext(aggregateType.Namespace, "Domain");

                if (!string.Equals(handlerContext, aggregateContext, StringComparison.Ordinal))
                {
                    violations.Add($"{handler.FullName} uses IRepository<{aggregateType.Name}> from context '{aggregateContext}' (handler context: '{handlerContext}')");
                }
            }
        }

        Assert.True(!violations.Any(),
            "CommandHandlers must use IRepository of their own bounded context:\n - " + string.Join("\n - ", violations));
    }
}

