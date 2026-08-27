using CampaignApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampaignApp.Infrastructure.Persistence.Configurations;

public class QuestNpcConfiguration : IEntityTypeConfiguration<QuestNpc>
{
    public void Configure(EntityTypeBuilder<QuestNpc> builder)
    {
        builder.HasKey(qn => qn.Id);

        builder.Property(qn => qn.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(qn => qn.Notes)
            .HasMaxLength(1000);

        // Disallow duplicate relationships for the same Quest+Npc pair
        // (also serves QuestId-only lookups as this index's leading column).
        builder.HasIndex(qn => new { qn.QuestId, qn.NpcId })
            .IsUnique();

        // Npc-side reverse lookups (GetAllForNpcAsync).
        builder.HasIndex(qn => qn.NpcId);

        builder.HasOne(qn => qn.Quest)
            .WithMany()
            .HasForeignKey(qn => qn.QuestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(qn => qn.Npc)
            .WithMany()
            .HasForeignKey(qn => qn.NpcId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
