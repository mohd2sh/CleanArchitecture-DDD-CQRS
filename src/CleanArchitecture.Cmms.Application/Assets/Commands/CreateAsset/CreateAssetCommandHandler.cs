using CleanArchitecture.Cmms.Application.Abstractions.Persistence.Repositories;
using CleanArchitecture.Cmms.Domain.Assets;
using CleanArchitecture.Cmms.Domain.Assets.ValueObjects;

namespace CleanArchitecture.Cmms.Application.Assets.Commands.CreateAsset
{
    internal sealed class CreateAssetCommandHandler
     : ICommandHandler<CreateAssetCommand, Result<Guid>>
    {
        private readonly IRepository<Asset, Guid> _repository;

        public CreateAssetCommandHandler(IRepository<Asset, Guid> repository)
        {
            _repository = repository;
        }

        public async Task<Result<Guid>> Handle(CreateAssetCommand request, CancellationToken cancellationToken)
        {

            var tag = AssetTag.Create(request.TagCode);
            var location = AssetLocation.Create(request.Site, request.Area, request.Zone);

            var asset = Asset.Create(request.Name, request.Type, tag, location);

            await _repository.AddAsync(asset, cancellationToken);

            return asset.Id;
        }
    }
}
