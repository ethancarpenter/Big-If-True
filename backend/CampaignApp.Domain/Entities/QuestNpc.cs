using CampaignApp.Domain.Common;
using CampaignApp.Domain.Enums;

namespace CampaignApp.Domain.Entities;

public class QuestNpc : AuditableEntity
{
    public Guid QuestId { get; set; }
    public Quest Quest { get; set; } = null!;
    public Guid NpcId { get; set; }
    public Npc Npc { get; set; } = null!;
    public QuestNpcRole Role { get; set; }
    public string? Notes { get; set; }
}
