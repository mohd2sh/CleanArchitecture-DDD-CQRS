using System.Reflection;
using CleanArchitecture.Cmms.Application.Abstractions.Messaging;
using CleanArchitecture.Cmms.Application.Abstractions.Persistence.Repositories;
using CleanArchitecture.Cmms.Application.Behaviors;
using CleanArchitecture.Cmms.Application.Primitives;
using CleanArchitecture.Cmms.Domain.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using NetArchTest.Rules;

namespace CleanArchitecture.Cmms.Application.UnitTests.ArchitectureTests;

public class ApplicationArchitectureTests
{
    private static readonly Assembly ApplicationAssembly = typeof(ServiceCollectionExtensions).Assembly;

    [Fact]
    public void Application_Should_Not_Depend_On_Infrastructure_Or_Api()
    {
        // Arrange
        var forbiddenAssemblies = new[]
        {
            typeof(Infrastructure.ServiceCollectionExtensions).Assembly.GetName().Name!,
        };

        // Act
        var result = Types
            .InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenAssemblies)
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            "Application layer must not depend on Infrastructure or API.");
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
                var hasInitOnlySetter = p.SetMethod.ReturnParameter
                    .GetRequiredCustomModifiers()
                    .Any(m => m.Name == "IsExternalInit");

                return !hasInitOnlySetter;
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
        var writeRepoType = typeof(Abstractions.Persistence.Repositories.IRepository<,>);
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
            $"Query handlers should not depend on IRepository (write-side). Found: {string.Join(", ", invalid.Select(t => t.Name))}");
    }

    [Fact]
    public void CommandHandlers_Should_Not_Use_IReadRepository()
    {
        // Arrange
        var readRepoType = typeof(Abstractions.Persistence.Repositories.IReadRepository);
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
            $"Command handlers should not depend on IReadRepository (read-side). Found: {string.Join(", ", invalid.Select(t => t.Name))}");
    }

    [Fact]
    public void Handlers_Should_Not_Return_Domain_Types()
    {
        // Arrange
        var domainAssembly = typeof(IEntity<>).Assembly;
        var domainTypes = domainAssembly.GetTypes()
            .Where(t => t.IsAssignableTo(typeof(ValueObject)) ||
                        t.IsAssignableTo(typeof(IAggregateRoot)) ||
                        t.BaseType != null && t.BaseType.IsGenericType &&
                         t.BaseType.GetGenericTypeDefinition() == typeof(IEntity<>))
            .Select(t => t.FullName)
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
                return domainTypes.Contains(returnType.FullName);
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

    /// <summary>
    /// Ensures that each bounded context (feature) within the Application layer 
    /// is isolated and does not depend on other Application features. 
    /// 
    /// The test dynamically discovers all bounded contexts by scanning aggregate roots 
    /// in the Domain layer that implement <see cref="IAggregateRoot"/>. 
    /// For each discovered feature, it verifies that its corresponding Application namespace 
    /// (e.g., Application.WorkOrders) has no compile-time dependencies on other feature namespaces 
    /// (e.g., Application.Technicians, Application.Assets). 
    /// 
    /// This enforces bounded context isolation within the Application layer and 
    /// prevents cross-feature coupling or layer corruption, maintaining a clean modular architecture.
    /// </summary>
    [Fact]
    public void Application_Features_Should_Be_BoundedContexts()
    {
        // Arrange
        var domainAssembly = typeof(IAggregateRoot).Assembly;
        var applicationAssembly = typeof(ICommandHandler<,>).Assembly;

        // Dynamically resolve base namespaces (strip ".Domain" or ".Application")
        var domainBaseNamespace = typeof(IAggregateRoot).Namespace!.Split('.')[..^1]; // remove ".Abstractions"
        var baseNamespace = string.Join(".", domainBaseNamespace);

        // Discover feature namespaces dynamically from aggregate roots
        var featureNamespaces = domainAssembly.GetTypes()
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
                .InAssembly(applicationAssembly)
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
    public void ServiceCollectionExtensions_ShouldRegister_DomainEventsPipelineAfterTransactionPipeline()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplication();

        // Get all pipeline registrations
        var pipelineBehaviors = services
            .Where(sd => sd.ServiceType.IsGenericType &&
                         sd.ServiceType.GetGenericTypeDefinition() == typeof(ICommandPipeline<,>))
            .ToList();

        // Find the indices using typeof instead of strings
        var transactionIndex = pipelineBehaviors
            .FindIndex(sd => sd.ImplementationType?.GetGenericTypeDefinition() == typeof(TransactionCommandPipeline<,>));

        var domainEventsIndex = pipelineBehaviors
            .FindIndex(sd => sd.ImplementationType?.GetGenericTypeDefinition() == typeof(DomainEventsPipeline<,>));

        // Assert
        Assert.True(transactionIndex >= 0, "TransactionCommandPipeline must be registered");
        Assert.True(domainEventsIndex >= 0, "DomainEventsPipeline must be registered");

        // Critical: DomainEventsPipeline must come AFTER TransactionCommandPipeline
        Assert.True(domainEventsIndex > transactionIndex,
            $"CRITICAL: DomainEventsPipeline (index {domainEventsIndex}) must be registered AFTER " +
            $"TransactionCommandPipeline (index {transactionIndex}).\n\n" +
            $"Current order will cause domain events to execute OUTSIDE the transaction,\n" +
            $"breaking ACID guarantees and preventing rollback on event handler failures.\n\n" +
            $"Fix: In ServiceCollectionExtensions, ensure\n" +
            $"TransactionCommandPipeline is registered BEFORE DomainEventsPipeline.");
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
            var handlerContext = ExtractBoundedContext(handler.Namespace, "Application");

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
                var aggregateContext = ExtractBoundedContext(aggregateType.Namespace, "Domain");

                if (!string.Equals(handlerContext, aggregateContext, StringComparison.Ordinal))
                {
                    violations.Add($"{handler.FullName} uses IRepository<{aggregateType.Name}> from context '{aggregateContext}' (handler context: '{handlerContext}')");
                }
            }
        }

        Assert.True(!violations.Any(),
            "CommandHandlers must use IRepository of their own bounded context:\n - " + string.Join("\n - ", violations));
    }

    private static string ExtractBoundedContext(string? ns, string marker)
    {
        // Example:
        // CleanArchitecture.Cmms.Application.Assets.Commands.UpdateAssetLocation  -> marker=Application -> Assets
        // CleanArchitecture.Cmms.Domain.Assets.Entities.Asset                     -> marker=Domain -> Assets
        if (string.IsNullOrWhiteSpace(ns)) return string.Empty;

        var parts = ns.Split('.');
        var idx = Array.FindIndex(parts, p => p.Equals(marker, StringComparison.Ordinal));
        if (idx < 0 || idx + 1 >= parts.Length) return string.Empty;

        return parts[idx + 1]; // token immediately after Application/Domain
    }
}
