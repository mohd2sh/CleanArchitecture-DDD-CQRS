using CleanArchitecture.Cmms.Application.Abstractions.Persistence.Repositories;
using CleanArchitecture.Cmms.Application.Assets.Dtos;
using CleanArchitecture.Cmms.Domain.Assets;
using CleanArchitecture.Cmms.Application.Assets;

namespace CleanArchitecture.Cmms.Application.Assets.Queries.GetAssetById
{
    internal sealed class GetAssetByIdQueryHandler
      : IQueryHandler<GetAssetByIdQuery, Result<AssetDto>>
    {
        private readonly IReadRepository<Asset, Guid> _repository;

        public GetAssetByIdQueryHandler(IReadRepository<Asset, Guid> repository)
        {
            _repository = repository;
        }

        public async Task<Result<AssetDto>> Handle(GetAssetByIdQuery request, CancellationToken cancellationToken)
        {
            var asset = await _repository.GetByIdAsync(request.AssetId, cancellationToken);
            if (asset is null)
                return AssetErrors.NotFound;

            var dto = new AssetDto
            {
                Id = asset.Id,
                Name = asset.Name,
                Type = asset.Type,
                TagValue = asset.Tag.Value,
                Site = asset.Location.Site,
                Area = asset.Location.Area,
                Zone = asset.Location.Zone,
                Status = asset.Status.ToString(),
                TotalMaintenanceRecords = asset.MaintenanceRecords.Count
            };

            return dto;
        }
    }

}
