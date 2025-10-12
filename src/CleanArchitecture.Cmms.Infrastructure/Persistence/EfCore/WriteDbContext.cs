using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Cmms.Infrastructure.Persistence.EfCore;

public sealed class WriteDbContext : CmmsDbContextBase
{
    public WriteDbContext(DbContextOptions<WriteDbContext> options) : base(options) { }

}
