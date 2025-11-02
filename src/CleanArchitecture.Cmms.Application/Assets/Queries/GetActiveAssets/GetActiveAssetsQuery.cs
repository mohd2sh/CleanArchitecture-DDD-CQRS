using CleanArchitecture.Cmms.Application.Assets.Dtos;
using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Application.Abstractions.Query;

namespace CleanArchitecture.Cmms.Application.Assets.Queries.GetActiveAssets;

public sealed record GetActiveAssetsQuery(PaginationParam Pagination)
: IQuery<Result<PaginatedList<AssetDto>>>;
