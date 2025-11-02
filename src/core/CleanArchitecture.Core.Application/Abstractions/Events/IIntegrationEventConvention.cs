namespace CleanArchitecture.Core.Application.Abstractions.Events;

/// <summary>
/// Convention for identifying integration event types.
/// </summary>
public interface IIntegrationEventConvention
{
    /// <summary>
    /// Determines if the specified type is considered an integration event.
    /// </summary>
    bool IsIntegrationEvent(Type type);
}

