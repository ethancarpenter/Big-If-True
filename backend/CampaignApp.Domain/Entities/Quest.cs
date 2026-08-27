using CampaignApp.Domain.Common;
using CampaignApp.Domain.Enums;

namespace CampaignApp.Domain.Entities;

public class Quest : AuditableEntity
{
    public Guid CampaignId { get; set; }
    public Campaign Campaign { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public QuestStatus Status { get; set; } = QuestStatus.Planned;
    public QuestType QuestType { get; set; }
    public int? RecommendedLevelMin { get; set; }
    public int? RecommendedLevelMax { get; set; }
    public string? DmNotes { get; set; }
    public List<QuestObjective> Objectives { get; set; } = [];
}
