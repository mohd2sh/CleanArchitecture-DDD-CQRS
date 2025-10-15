using CleanArchitecture.Cmms.Application.Assets.Dtos;

namespace CleanArchitecture.Cmms.Application.Assets.Queries.GetActiveAssets
{
    public sealed record GetActiveAssetsQuery(PaginationParam Pagination)
    : IQuery<Result<PaginatedList<AssetDto>>>;
}
