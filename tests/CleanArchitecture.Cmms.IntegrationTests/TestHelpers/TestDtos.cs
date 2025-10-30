namespace CleanArchitecture.Cmms.IntegrationTests.TestHelpers;

/// <summary>
/// Test-only DTOs for deserializing API responses without requiring JsonConstructor attributes.
/// </summary>

public class ResultDto
{
    public bool IsSuccess { get; set; }
    public bool IsFailure { get; set; }
    public ErrorDto Error { get; set; }
}

public class ResultDto<T>
{
    public bool IsSuccess { get; set; }
    public bool IsFailure { get; set; }
    public ErrorDto? Error { get; set; }
    public T Value { get; set; }
}

public class ErrorDto
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int Type { get; set; }
}

public class PaginatedListDto<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

