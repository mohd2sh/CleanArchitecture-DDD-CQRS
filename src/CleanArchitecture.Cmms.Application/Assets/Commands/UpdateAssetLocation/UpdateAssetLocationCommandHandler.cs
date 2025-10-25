using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;
using CleanArchitecture.Cmms.Domain.Assets;
using CleanArchitecture.Cmms.Domain.Assets.ValueObjects;

namespace CleanArchitecture.Cmms.Application.Assets.Commands.UpdateAssetLocation
{
    internal sealed class UpdateAssetLocationCommandHandler
     : ICommandHandler<UpdateAssetLocationCommand, Result>
    {
        private readonly IRepository<Asset, Guid> _repository;

        public UpdateAssetLocationCommandHandler(IRepository<Asset, Guid> repository)
        {
            _repository = repository;
        }

        public async Task<Result> Handle(UpdateAssetLocationCommand request, CancellationToken cancellationToken)
        {
            var asset = await _repository.GetByIdAsync(request.AssetId, cancellationToken);

            if (asset is null)
                return AssetErrors.NotFound;

            var newLocation = AssetLocation.Create(request.Site, request.Area, request.Zone);

            asset.UpdateLocation(newLocation);

            await _repository.UpdateAsync(asset, cancellationToken);

            return Result.Success();
        }
    }
}
