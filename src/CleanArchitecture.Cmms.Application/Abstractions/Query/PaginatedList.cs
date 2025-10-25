namespace CleanArchitecture.Cmms.Application.Abstractions.Query
{
    public sealed class PaginatedList<T>
    {
        public IReadOnlyList<T> Items { get; }
        public int TotalCount { get; }
        public int PageNumber { get; }
        public int PageSize { get; }

        private PaginatedList(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
        {
            Items = items;
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }

        public static PaginatedList<T> Create(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
            => new(items, totalCount, pageNumber, pageSize);

        public static PaginatedList<T> CreateFromOffset(IReadOnlyList<T> items, int totalCount, int? skip, int? take)
        {
            var pageNumber = skip.HasValue && take.HasValue && take.Value > 0 ? skip.Value / take.Value + 1 : 1;

            var pageSize = take ?? totalCount;

            return new PaginatedList<T>(items, totalCount, pageNumber, pageSize);
        }

        public PaginatedList<TResult> ToNew<TResult>(IReadOnlyList<TResult> newItems)
        {
            return new PaginatedList<TResult>(newItems, TotalCount, PageNumber, PageSize);
        }
    }
}
