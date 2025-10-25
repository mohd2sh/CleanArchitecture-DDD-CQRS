using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Application.Abstractions.Persistence;
using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;
using CleanArchitecture.Core.Application.Abstractions.Query;
using CleanArchitecture.Cmms.Application.Assets.Dtos;
using CleanArchitecture.Cmms.Domain.Assets;
using CleanArchitecture.Cmms.Domain.Assets.Enums;

namespace CleanArchitecture.Cmms.Application.Assets.Queries.GetActiveAssets
{
    internal sealed class GetActiveAssetsQueryHandler
    : IQueryHandler<GetActiveAssetsQuery, Result<PaginatedList<AssetDto>>>
    {
        private readonly IReadRepository<Asset, Guid> _repository;

        public GetActiveAssetsQueryHandler(IReadRepository<Asset, Guid> repository)
        {
            _repository = repository;
        }

        public async Task<Result<PaginatedList<AssetDto>>> Handle(
            GetActiveAssetsQuery request,
            CancellationToken cancellationToken)
        {

            var criteria = Criteria<Asset>.New()
                .Where(a => a.Status == AssetStatus.Active)
                .OrderByAsc(a => a.Name)
                .Skip(request.Pagination.Skip)
                .Take(request.Pagination.Take)
                .Build();

            var paginatedAssets = await _repository.ListAsync(criteria, cancellationToken);

            var dtos = paginatedAssets.Items.Select(a => new AssetDto
            {
                Id = a.Id,
                Name = a.Name,
                Type = a.Type,
                TagValue = a.Tag.Value,
                Site = a.Location.Site,
                Area = a.Location.Area,
                Zone = a.Location.Zone,
                Status = a.Status.ToString(),
                TotalMaintenanceRecords = a.MaintenanceRecords.Count
            }).ToList();

            return paginatedAssets.ToNew(dtos);
        }
    }

}
