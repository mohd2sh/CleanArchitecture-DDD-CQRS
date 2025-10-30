using CleanArchitecture.Cmms.Application.Assets.Dtos;
using CleanArchitecture.Core.Application.Abstractions.Common;

namespace CleanArchitecture.Cmms.Application.Assets.Queries.GetAssetById
{
    public sealed record GetAssetByIdQuery(Guid AssetId) : IQuery<Result<AssetDto>>;

}
