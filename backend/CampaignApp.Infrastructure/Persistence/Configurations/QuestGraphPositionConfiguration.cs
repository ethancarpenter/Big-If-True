using CampaignApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampaignApp.Infrastructure.Persistence.Configurations;

public class QuestGraphPositionConfiguration : IEntityTypeConfiguration<QuestGraphPosition>
{
    public void Configure(EntityTypeBuilder<QuestGraphPosition> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.X).IsRequired();
        builder.Property(p => p.Y).IsRequired();

        builder.HasIndex(p => p.QuestId)
            .IsUnique();

        builder.HasOne(p => p.Quest)
            .WithMany()
            .HasForeignKey(p => p.QuestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
