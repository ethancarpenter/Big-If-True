using CampaignApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampaignApp.Infrastructure.Persistence.Configurations;

public class QuestLocationConfiguration : IEntityTypeConfiguration<QuestLocation>
{
    public void Configure(EntityTypeBuilder<QuestLocation> builder)
    {
        builder.HasKey(ql => ql.Id);

        builder.Property(ql => ql.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(ql => ql.Notes)
            .HasMaxLength(1000);

        // Disallow duplicate relationships for the same Quest+Location pair
        // (also serves QuestId-only lookups as this index's leading column).
        builder.HasIndex(ql => new { ql.QuestId, ql.LocationId })
            .IsUnique();

        // Location-side reverse lookups (GetAllForLocationAsync).
        builder.HasIndex(ql => ql.LocationId);

        builder.HasOne(ql => ql.Quest)
            .WithMany()
            .HasForeignKey(ql => ql.QuestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ql => ql.Location)
            .WithMany()
            .HasForeignKey(ql => ql.LocationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
