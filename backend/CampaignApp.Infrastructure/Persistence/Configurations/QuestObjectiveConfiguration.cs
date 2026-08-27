using CampaignApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampaignApp.Infrastructure.Persistence.Configurations;

public class QuestObjectiveConfiguration : IEntityTypeConfiguration<QuestObjective>
{
    public void Configure(EntityTypeBuilder<QuestObjective> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(o => o.IsCompleted)
            .IsRequired();

        builder.Property(o => o.SortOrder)
            .IsRequired();

        builder.HasIndex(o => o.QuestId);

        builder.HasOne(o => o.Quest)
            .WithMany(q => q.Objectives)
            .HasForeignKey(o => o.QuestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
