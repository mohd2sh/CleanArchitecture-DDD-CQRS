using CleanArchitecture.Cmms.Domain.Assets;
using CleanArchitecture.Cmms.Domain.Technicians;
using CleanArchitecture.Cmms.Domain.WorkOrders;
using CleanArchitecture.Cmms.Infrastructure.Persistence.EfCore;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Cmms.Infrastructure.Persistence
{
    public abstract class CmmsDbContextBase : DbContext
    {
        protected CmmsDbContextBase(DbContextOptions options) : base(options) { }

        internal DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
        internal DbSet<Technician> Technicians => Set<Technician>();
        internal DbSet<Asset> Assets => Set<Asset>();



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CmmsDbContextBase).Assembly);
            modelBuilder.ApplyModelConventions();
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.ApplyGlobalConventions();
        }
    }
}
