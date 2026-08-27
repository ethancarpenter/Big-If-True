using CampaignApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampaignApp.Infrastructure.Persistence.Configurations;

public class NpcConfiguration : IEntityTypeConfiguration<Npc>
{
    public void Configure(EntityTypeBuilder<Npc> builder)
    {
        builder.HasKey(n => n.Id);

        builder.Property(n => n.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(n => n.Species)
            .HasMaxLength(100);

        builder.Property(n => n.Gender)
            .HasMaxLength(100);

        builder.Property(n => n.Class)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(n => n.Alignment)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(n => n.Occupation)
            .HasMaxLength(200);

        builder.Property(n => n.Disposition)
            .HasMaxLength(100);

        builder.Property(n => n.Description)
            .HasMaxLength(2000);

        builder.Property(n => n.DmNotes)
            .HasMaxLength(2000);

        builder.Property(n => n.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(n => n.PortraitUrl)
            .HasMaxLength(500);

        builder.HasIndex(n => n.CampaignId);

        builder.HasOne(n => n.Campaign)
            .WithMany()
            .HasForeignKey(n => n.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
