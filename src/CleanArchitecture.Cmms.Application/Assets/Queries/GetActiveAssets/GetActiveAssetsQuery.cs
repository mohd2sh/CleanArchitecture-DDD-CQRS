using CleanArchitecture.Cmms.Application.Assets.Dtos;

namespace CleanArchitecture.Cmms.Application.Assets.Queries.GetActiveAssets
{
    public sealed record GetActiveAssetsQuery(int PageNumber = 1, int PageSize = 20)
    : IQuery<Result<PaginatedList<AssetDto>>>;
}
