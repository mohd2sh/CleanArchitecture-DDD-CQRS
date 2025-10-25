using CleanArchitecture.Cmms.Application.Abstractions.Common;
using CleanArchitecture.Cmms.Application.Abstractions.Query;
using CleanArchitecture.Cmms.Application.Assets.Dtos;

namespace CleanArchitecture.Cmms.Application.Assets.Queries.GetActiveAssets
{
    public sealed record GetActiveAssetsQuery(PaginationParam Pagination)
    : IQuery<Result<PaginatedList<AssetDto>>>;
}
