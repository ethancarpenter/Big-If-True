using CampaignApp.Domain.Common;

namespace CampaignApp.Domain.Entities;

public class QuestObjective : AuditableEntity
{
    public Guid QuestId { get; set; }
    public Quest Quest { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public int SortOrder { get; set; }
}
