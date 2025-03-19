namespace CleanArchitecture.Cmms.Application.Primitives
{
    public sealed record PaginationParams(int PageNumber = 1, int PageSize = 20);
}
