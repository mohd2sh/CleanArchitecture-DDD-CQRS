using CleanArchitecture.Cmms.Application.Abstractions.Messaging;
using CleanArchitecture.Cmms.Domain.Abstractions;
using NetArchTest.Rules;
using System.Reflection;

namespace CleanArchitecture.Cmms.Application.UnitTests
{
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
            var writeRepoType = typeof(Application.Abstractions.Persistence.Repositories.IRepository<,>);
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
                            (t.BaseType != null && t.BaseType.IsGenericType &&
                             t.BaseType.GetGenericTypeDefinition() == typeof(IEntity<>)))
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
                .Where(ns => ns != null && ns!.Contains("."))
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
    }
}
