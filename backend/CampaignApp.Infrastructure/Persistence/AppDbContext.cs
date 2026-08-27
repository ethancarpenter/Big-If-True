using CampaignApp.Domain.Entities;
using CampaignApp.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace CampaignApp.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Npc> Npcs => Set<Npc>();
    public DbSet<NpcLocation> NpcLocations => Set<NpcLocation>();
    public DbSet<Quest> Quests => Set<Quest>();
    public DbSet<QuestObjective> QuestObjectives => Set<QuestObjective>();
    public DbSet<QuestNpc> QuestNpcs => Set<QuestNpc>();
    public DbSet<QuestLocation> QuestLocations => Set<QuestLocation>();
    public DbSet<QuestConnection> QuestConnections => Set<QuestConnection>();
    public DbSet<QuestGraphPosition> QuestGraphPositions => Set<QuestGraphPosition>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new CampaignConfiguration());
        modelBuilder.ApplyConfiguration(new CityConfiguration());
        modelBuilder.ApplyConfiguration(new LocationConfiguration());
        modelBuilder.ApplyConfiguration(new NpcConfiguration());
        modelBuilder.ApplyConfiguration(new NpcLocationConfiguration());
        modelBuilder.ApplyConfiguration(new QuestConfiguration());
        modelBuilder.ApplyConfiguration(new QuestObjectiveConfiguration());
        modelBuilder.ApplyConfiguration(new QuestNpcConfiguration());
        modelBuilder.ApplyConfiguration(new QuestLocationConfiguration());
        modelBuilder.ApplyConfiguration(new QuestConnectionConfiguration());
        modelBuilder.ApplyConfiguration(new QuestGraphPositionConfiguration());
    }
}
