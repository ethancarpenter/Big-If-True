using CampaignApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampaignApp.Infrastructure.Persistence.Configurations;

public class NpcLocationConfiguration : IEntityTypeConfiguration<NpcLocation>
{
    public void Configure(EntityTypeBuilder<NpcLocation> builder)
    {
        builder.HasKey(nl => nl.Id);

        builder.Property(nl => nl.RelationshipType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(nl => nl.IsPrimary)
            .IsRequired();

        // Disallow duplicate relationships for the same Npc+Location pair
        // (also serves NpcId-only lookups as this index's leading column).
        builder.HasIndex(nl => new { nl.NpcId, nl.LocationId })
            .IsUnique();

        // Location-side lookups (GetAllForLocationAsync).
        builder.HasIndex(nl => nl.LocationId);

        // At most one primary Location per NPC - a Postgres partial unique
        // index. This is the final DB-level safeguard; NpcLocationRepository
        // .SavePrimarySwitchAsync is what keeps normal application usage
        // from ever hitting it.
        builder.HasIndex(nl => nl.NpcId)
            .IsUnique()
            .HasFilter("\"IsPrimary\" = true")
            .HasDatabaseName("IX_NpcLocations_NpcId_Primary");

        builder.HasOne(nl => nl.Npc)
            .WithMany()
            .HasForeignKey(nl => nl.NpcId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(nl => nl.Location)
            .WithMany()
            .HasForeignKey(nl => nl.LocationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
