using CampaignApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampaignApp.Infrastructure.Persistence.Configurations;

public class QuestConfiguration : IEntityTypeConfiguration<Quest>
{
    public void Configure(EntityTypeBuilder<Quest> builder)
    {
        builder.HasKey(q => q.Id);

        builder.Property(q => q.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(q => q.Description)
            .HasMaxLength(2000);

        builder.Property(q => q.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(q => q.QuestType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(q => q.DmNotes)
            .HasMaxLength(2000);

        builder.HasIndex(q => q.CampaignId);

        builder.HasOne(q => q.Campaign)
            .WithMany()
            .HasForeignKey(q => q.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
