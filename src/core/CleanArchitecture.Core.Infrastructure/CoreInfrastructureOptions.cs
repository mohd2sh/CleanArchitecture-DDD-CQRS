using CleanArchitecture.Core.Application.Abstractions.Events;

namespace CleanArchitecture.Core.Infrastructure;

/// <summary>
/// Configuration options for Core Infrastructure services.
/// </summary>
public class CoreInfrastructureOptions
{
    /// <summary>
    /// Gets or sets a custom integration event convention implementation.
    /// If set, this will be registered instead of the default convention.
    /// </summary>
    public IIntegrationEventConvention? CustomConvention { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to register the default integration event convention.
    /// Defaults to <c>true</c>. Set to <c>false</c> if you want to provide your own convention.
    /// </summary>
    public bool RegisterConvention { get; set; } = true;
}

