using CampaignApp.Domain.Common;

namespace CampaignApp.Domain.Entities;

public class Campaign : AuditableEntity
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }
}
