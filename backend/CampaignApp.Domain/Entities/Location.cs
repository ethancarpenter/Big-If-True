using CampaignApp.Domain.Common;
using CampaignApp.Domain.Enums;

namespace CampaignApp.Domain.Entities;

public class Location : AuditableEntity
{
    public Guid CampaignId { get; set; }
    public Campaign Campaign { get; set; } = null!;
    public Guid CityId { get; set; }
    public City City { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public LocationType Type { get; set; }
    public string? Description { get; set; }
    public string? DmNotes { get; set; }
}
