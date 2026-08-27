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
    public DbSet<City> Cities => Set<City>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Npc> Npcs => Set<Npc>();
    public DbSet<NpcLocation> NpcLocations => Set<NpcLocation>();
    public DbSet<Quest> Quests => Set<Quest>();
    public DbSet<QuestObjective> QuestObjectives => Set<QuestObjective>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new CampaignConfiguration());
        modelBuilder.ApplyConfiguration(new CityConfiguration());
        modelBuilder.ApplyConfiguration(new LocationConfiguration());
        modelBuilder.ApplyConfiguration(new NpcConfiguration());
        modelBuilder.ApplyConfiguration(new NpcLocationConfiguration());
        modelBuilder.ApplyConfiguration(new QuestConfiguration());
        modelBuilder.ApplyConfiguration(new QuestObjectiveConfiguration());
    }
}
