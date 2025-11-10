using System.Reflection;
using CleanArchitecture.Cmms.Application.Behaviors;
using CleanArchitecture.Core.Application.Abstractions.Messaging;
using CleanArchitecture.Core.ArchitectureTests.Application;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Cmms.Application.UnitTests.ArchitectureTests;

/// <summary>
/// Application architecture tests using the reusable base class from Core.ArchitectureTests.
/// </summary>
public class ApplicationArchitectureTests : ApplicationArchitectureTestBase
{
    protected override Assembly ApplicationAssembly => typeof(ServiceCollectionExtensions).Assembly;

    protected override Assembly DomainAssembly => typeof(Domain.Assets.Asset).Assembly;

    protected override Assembly[] ForbiddenAssemblies => new[]
    {
        typeof(Infrastructure.ServiceCollectionExtensions).Assembly
    };

    /// <summary>
    /// Usage-specific test that validates pipeline registration order.
    /// </summary>
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

        Assert.True(domainEventsIndex > transactionIndex,
            $"CRITICAL: DomainEventsPipeline (index {domainEventsIndex}) must be registered AFTER " +
            $"TransactionCommandPipeline (index {transactionIndex}).\n\n" +
            $"Current order will cause domain events to execute OUTSIDE the transaction,\n" +
            $"breaking ACID guarantees and preventing rollback on event handler failures.\n\n" +
            $"Fix: In ServiceCollectionExtensions, ensure\n" +
            $"TransactionCommandPipeline is registered BEFORE DomainEventsPipeline.");
    }
}
