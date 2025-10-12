using CleanArchitecture.Cmms.Domain.Assets;
using CleanArchitecture.Cmms.Domain.Assets.ValueObjects;
using CleanArchitecture.Cmms.Domain.Technicians;
using CleanArchitecture.Cmms.Domain.Technicians.ValueObjects;
using CleanArchitecture.Cmms.Domain.WorkOrders;
using CleanArchitecture.Cmms.Domain.WorkOrders.ValueObjects;
using CleanArchitecture.Cmms.Infrastructure.Persistence.EfCore;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Cmms.Infrastructure.Persistence
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(WriteDbContext context, CancellationToken ct = default)
        {
            await context.Database.EnsureCreatedAsync(ct);

            if (!await context.Technicians.AnyAsync(ct))
            {
                var techs = new List<Technician>
            {
                Technician.Create("John Doe", SkillLevel.Journeyman),
                Technician.Create("Jane Smith", SkillLevel.Master),
                Technician.Create("Tom Wilson", SkillLevel.Apprentice)
            };
                await context.Technicians.AddRangeAsync(techs, ct);
            }

            if (!await context.Assets.AnyAsync(ct))
            {
                var assets = new List<Asset>
            {
                Asset.Create("Boiler Pump", "Mechanical",
                    AssetTag.Create("ASSET-1001"),
                    AssetLocation.Create("Plant-A", "Floor-1", "Zone-3")),
                Asset.Create("HVAC Unit", "Electrical",
                    AssetTag.Create("ASSET-2002"),
                    AssetLocation.Create("Plant-B", "Roof", "Zone-1"))
            };
                await context.Assets.AddRangeAsync(assets, ct);
            }

            if (!await context.WorkOrders.AnyAsync(ct))
            {
                var workOrders = new List<WorkOrder>
            {
                WorkOrder.Create("Replace filter on HVAC Unit",
                    new Location("Plant-B", "Roof", "Zone-1")),
                WorkOrder.Create("Inspect Boiler Pump",
                    new Location("Plant-A", "Floor-1", "Zone-3"))
            };
                await context.WorkOrders.AddRangeAsync(workOrders, ct);
            }

            await context.SaveChangesAsync(ct);
        }
    }
}
