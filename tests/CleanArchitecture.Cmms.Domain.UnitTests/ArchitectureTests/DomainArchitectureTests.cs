using System.Reflection;
using CleanArchitecture.Core.ArchitectureTests.Domain;

namespace CleanArchitecture.Cmms.Domain.UnitTests.ArchitectureTests;

/// <summary>
/// Domain architecture tests using the reusable base class from Core.ArchitectureTests.
/// </summary>
public class DomainArchitectureTests : DomainArchitectureTestBase
{
    protected override Assembly DomainAssembly => typeof(Domain.Assets.Asset).Assembly;

    protected override Assembly[] ForbiddenAssemblies => new[]
    {
        typeof(Application.ServiceCollectionExtensions).Assembly,
        typeof(Infrastructure.ServiceCollectionExtensions).Assembly
    };
}
