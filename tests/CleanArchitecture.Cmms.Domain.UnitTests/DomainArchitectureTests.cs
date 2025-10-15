using CleanArchitecture.Cmms.Domain.Abstractions;
using NetArchTest.Rules;
using System.Reflection;

namespace CleanArchitecture.Cmms.Domain.UnitTests
{
    public class DomainArchitectureTests
    {
        private static readonly Assembly DomainAssembly = typeof(Assets.Asset).Assembly;

        [Fact]
        public void ValueObjects_Should_Be_Immutable()
        {
            // Arrange
            var valueObjectTypes = Types
                .InAssembly(DomainAssembly)
                .That()
                .Inherit(typeof(ValueObject))
                .GetTypes();

            // Act
            var invalid = valueObjectTypes
                .SelectMany(t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                .Where(p =>
                {
                    // If there is no setter, it's fine
                    if (p.SetMethod == null)
                        return false;

                    // If it's not public, it's fine
                    if (!p.SetMethod.IsPublic)
                        return false;

                    // If the setter is an 'init' accessor, it's fine (immutable)
                    var hasInitOnlySetter = p.SetMethod.ReturnParameter
                        .GetRequiredCustomModifiers()
                        .Any(m => m.Name == "IsExternalInit");

                    return !hasInitOnlySetter;
                })
                .ToList();

            // Assert
            Assert.True(!invalid.Any(),
                $"ValueObjects should be immutable (no public non-init setters). Found: {string.Join(", ", invalid.Select(p => p.DeclaringType?.Name + '.' + p.Name))}");
        }


        [Fact]
        public void DomainEvents_Should_Be_Sealed_And_EndWith_Event()
        {
            // Arrange
            var domainEventInterface = typeof(IDomainEvent);
            var eventTypes = Types
                .InAssembly(DomainAssembly)
                .That()
                .ImplementInterface(domainEventInterface)
                .GetTypes();

            // Act
            var notSealed = eventTypes.Where(t => !t.IsSealed).ToList();
            var invalidNames = eventTypes.Where(t => !t.Name.EndsWith("Event")).ToList();

            // Assert
            Assert.True(!notSealed.Any(),
                $"Domain events must be sealed. Found non-sealed: {string.Join(", ", notSealed.Select(t => t.Name))}");
            Assert.True(!invalidNames.Any(),
                $"Domain events must end with 'Event'. Found invalid names: {string.Join(", ", invalidNames.Select(t => t.Name))}");
        }


        [Fact]
        public void Entities_Should_Not_Reference_Other_AggregateRoots()
        {
            // Arrange
            var aggregateRootType = typeof(IAggregateRoot);
            var entityType = typeof(Entity<>);

            var aggregateRoots = Types
                .InAssembly(DomainAssembly)
                .That()
                .ImplementInterface(aggregateRootType)
                .GetTypes();

            var entities = Types
                .InAssembly(DomainAssembly)
                .That()
                .Inherit(entityType)
                .GetTypes();

            // Act
            var invalid = entities
                .SelectMany(e =>
                    e.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                     .Select(f => (Parent: e, Type: f.FieldType))
                    .Concat(
                        e.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                         .Select(p => (Parent: e, Type: p.PropertyType))
                    ))
                .Where(x => aggregateRoots.Contains(x.Type))
                .ToList();

            // Assert
            Assert.True(!invalid.Any(),
                $"Entities must not reference other AggregateRoots. Found invalid references: {string.Join(", ", invalid.Select(i => $"{i.Parent.Name}->{i.Type.Name}"))}");
        }

        [Fact]
        public void Entities_And_AggregateRoots_Should_Have_Private_Constructors()
        {
            // Arrange
            var entityType = typeof(Entity<>);
            var aggregateRootType = typeof(IAggregateRoot);

            var candidates = Types
                .InAssembly(DomainAssembly)
                .That()
                .Inherit(entityType)
                .Or()
                .ImplementInterface(aggregateRootType)
                .GetTypes();

            // Act
            var invalid = candidates
                .SelectMany(t => t.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
                .ToList();

            // Assert
            Assert.True(!invalid.Any(),
                $"Entities and AggregateRoots should not have public constructors. Found in: {string.Join(", ", invalid.Select(c => c.DeclaringType?.Name))}");
        }


        [Fact]
        public void Entities_Should_Have_Private_Parameterless_Constructor()
        {
            // Arrange
            var entityType = typeof(Entity<>);
            var entities = Types
                .InAssembly(DomainAssembly)
                .That()
                .Inherit(entityType)
                .GetTypes();

            // Act
            var invalid = entities
                .Where(t => !t.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
                              .Any(c => c.GetParameters().Length == 0))
                .ToList();

            // Assert
            Assert.True(!invalid.Any(),
                $"Entities should define a private parameterless constructor for EF. Missing in: {string.Join(", ", invalid.Select(t => t.Name))}");
        }

        [Fact]
        public void Domain_Types_Should_Be_Internal()
        {
            // Arrange
            var entityType = typeof(Entity<>);
            var aggregateRootType = typeof(IAggregateRoot);
            var valueObjectType = typeof(ValueObject);

            var domainTypes = Types
                .InAssembly(DomainAssembly)
                .That()
                .Inherit(entityType)
                .Or()
                .ImplementInterface(aggregateRootType)
                .Or()
                .Inherit(valueObjectType)
                .GetTypes();

            // Act
            var invalid = domainTypes.Where(t => t.IsPublic).ToList();

            // Assert
            Assert.True(!invalid.Any(),
                $"Domain types should be internal. Found public: {string.Join(", ", invalid.Select(t => t.Name))}");
        }


        [Fact]
        public void Domain_Should_Not_Depend_On_Other_Layers()
        {
            // Arrange
            var domainAssembly = typeof(Entity<>).Assembly;

            var applicationAssembly = typeof(Application.ServiceCollectionExtensions).Assembly;
            var infrastructureAssembly = typeof(Infrastructure.ServiceCollectionExtensions).Assembly;

            var forbiddenAssemblies = new[]
            {
                applicationAssembly.GetName().Name!,
                infrastructureAssembly.GetName().Name!,
            };

            // Act
            var result = Types
                .InAssembly(domainAssembly)
                .ShouldNot()
                .HaveDependencyOnAny(forbiddenAssemblies)
                .GetResult();

            // Assert
            Assert.True(result.IsSuccessful,
                $"Domain layer must not depend on: {string.Join(", ", forbiddenAssemblies)}");
        }


        [Fact]
        public void ValueObjects_Should_Be_Sealed()
        {
            // Arrange
            var valueObjectType = typeof(ValueObject);
            var valueObjects = Types
                .InAssembly(DomainAssembly)
                .That()
                .Inherit(valueObjectType)
                .GetTypes();

            // Act
            var invalid = valueObjects.Where(t => !t.IsSealed).ToList();

            // Assert
            Assert.True(!invalid.Any(),
                $"ValueObjects must be sealed. Found: {string.Join(", ", invalid.Select(t => t.Name))}");
        }
    }
}
