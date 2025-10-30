namespace CleanArchitecture.Core.Application.Abstractions.Query
{
    public sealed record PaginationParam(int PageNumber = 1, int PageSize = 20)
    {
        public int Skip => (PageNumber - 1) * PageSize;
        public int Take => PageSize;
    }
}
