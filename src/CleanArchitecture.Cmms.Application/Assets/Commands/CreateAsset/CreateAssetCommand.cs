using CleanArchitecture.Core.Application.Abstractions.Common;

namespace CleanArchitecture.Cmms.Application.Assets.Commands.CreateAsset
{
    public sealed record CreateAssetCommand(string Name,
        string Type,
        string TagCode,
        string Site,
        string Area,
        string Zone) : ICommand<Result<Guid>>;
}
