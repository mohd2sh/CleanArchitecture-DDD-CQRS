namespace CleanArchitecture.Cmms.Application.ErrorManagement.Models;

public record ApplicationErrorInfo
{
    public required string Code { get; init; }
    public required string Message { get; init; }
    public required string Type { get; init; }
    public required string Domain { get; init; }
    public required string FieldName { get; init; }
    public required string ClassName { get; init; }
}
