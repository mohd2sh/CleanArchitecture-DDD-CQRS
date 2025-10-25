using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Cmms.Application.Assets.Dtos;

namespace CleanArchitecture.Cmms.Application.Assets.Queries.GetAssetById
{
    public sealed record GetAssetByIdQuery(Guid AssetId) : IQuery<Result<AssetDto>>;

}
