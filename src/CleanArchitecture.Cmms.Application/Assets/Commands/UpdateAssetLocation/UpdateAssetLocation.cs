using CleanArchitecture.Core.Application.Abstractions.Common;

namespace CleanArchitecture.Cmms.Application.Assets.Commands.UpdateAssetLocation
{
    public sealed record UpdateAssetLocationCommand(
         Guid AssetId,
         string Site,
         string Area,
         string Zone) : ICommand<Result>;
}
