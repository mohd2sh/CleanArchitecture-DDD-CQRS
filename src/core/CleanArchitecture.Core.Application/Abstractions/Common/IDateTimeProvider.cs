namespace CleanArchitecture.Core.Application.Abstractions.Common;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
    DateTime Now { get; }
    DateOnly Today { get; }
}
