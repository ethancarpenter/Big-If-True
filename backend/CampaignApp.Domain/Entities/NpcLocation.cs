using CampaignApp.Domain.Common;
using CampaignApp.Domain.Enums;

namespace CampaignApp.Domain.Entities;

public class NpcLocation : AuditableEntity
{
    public Guid NpcId { get; set; }
    public Npc Npc { get; set; } = null!;
    public Guid LocationId { get; set; }
    public Location Location { get; set; } = null!;
    public NpcLocationRelationshipType RelationshipType { get; set; }
    public bool IsPrimary { get; set; }
}
