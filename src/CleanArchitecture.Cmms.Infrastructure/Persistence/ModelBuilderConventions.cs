using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Cmms.Infrastructure.Persistence;

public static class ModelBuilderConventions
{
    public static void ApplyModelConventions(this ModelBuilder b)
    {
        b.HasSequence<long>("workorder_hilo").StartsAt(1000).IncrementsBy(10);
    }

    public static void ApplyGlobalConventions(this ModelConfigurationBuilder c)
    {
        c.Properties<decimal>().HavePrecision(18, 2);
    }
}
