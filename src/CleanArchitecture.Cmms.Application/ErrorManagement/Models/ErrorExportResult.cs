namespace CleanArchitecture.Cmms.Application.ErrorManagement.Models;

public record ErrorExportResult
{
    public required Dictionary<string, DomainErrorInfo> DomainErrors { get; init; }
    public required Dictionary<string, ApplicationErrorInfo> ApplicationErrors { get; init; }
    public required DateTime Timestamp { get; init; }
}
