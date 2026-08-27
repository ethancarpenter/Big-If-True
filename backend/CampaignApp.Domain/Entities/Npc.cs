using CampaignApp.Domain.Common;
using CampaignApp.Domain.Enums;

namespace CampaignApp.Domain.Entities;

public class Npc : AuditableEntity
{
    public Guid CampaignId { get; set; }
    public Campaign Campaign { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string? Species { get; set; }
    public string? Gender { get; set; }
    public int? Age { get; set; }
    public NpcClass? Class { get; set; }
    public Alignment? Alignment { get; set; }
    public string? Occupation { get; set; }
    public string? Disposition { get; set; }
    public string? Description { get; set; }
    public string? DmNotes { get; set; }
    public NpcStatus Status { get; set; } = NpcStatus.Alive;
    public string? PortraitUrl { get; set; }
}
