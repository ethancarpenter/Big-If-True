using CampaignApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampaignApp.Infrastructure.Persistence.Configurations;

public class QuestConnectionConfiguration : IEntityTypeConfiguration<QuestConnection>
{
    public void Configure(EntityTypeBuilder<QuestConnection> builder)
    {
        builder.HasKey(qc => qc.Id);

        builder.Property(qc => qc.ConnectionType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasIndex(qc => new { qc.SourceQuestId, qc.TargetQuestId })
            .IsUnique();

        builder.HasIndex(qc => qc.TargetQuestId);

        builder.HasOne(qc => qc.SourceQuest)
            .WithMany()
            .HasForeignKey(qc => qc.SourceQuestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(qc => qc.TargetQuest)
            .WithMany()
            .HasForeignKey(qc => qc.TargetQuestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_QuestConnections_NoSelfLink", "\"SourceQuestId\" <> \"TargetQuestId\""));
    }
}
