using CampaignApp.Domain.Entities;
using CampaignApp.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace CampaignApp.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Campaign> Campaigns => Set<Campaign>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new CampaignConfiguration());
    }
}
