using CampaignApp.Domain.Common;

namespace CampaignApp.Domain.Entities;

public class City : AuditableEntity
{
    public Guid CampaignId { get; set; }
    public Campaign Campaign { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Population { get; set; }
    public string? Government { get; set; }
    public string? Region { get; set; }
    public string? Alignment { get; set; }
}
