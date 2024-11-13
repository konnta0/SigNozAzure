using Microsoft.EntityFrameworkCore;

namespace SampleWebApp;

public sealed class ExampleDbContext(DbContextOptions<ExampleDbContext> dbContextOptions) : DbContext(dbContextOptions)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}