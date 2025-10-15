namespace CleanArchitecture.Cmms.Application.Primitives
{
    public sealed record PaginationParam(int PageNumber = 1, int PageSize = 20)
    {
        public int Skip => (PageNumber - 1) * PageSize;
        public int Take => PageSize;
    }
}
