using CampaignApp.Domain.Common;
using CampaignApp.Domain.Enums;

namespace CampaignApp.Domain.Entities;

public class QuestLocation : AuditableEntity
{
    public Guid QuestId { get; set; }
    public Quest Quest { get; set; } = null!;
    public Guid LocationId { get; set; }
    public Location Location { get; set; } = null!;
    public QuestLocationRole Role { get; set; }
    public string? Notes { get; set; }
}
