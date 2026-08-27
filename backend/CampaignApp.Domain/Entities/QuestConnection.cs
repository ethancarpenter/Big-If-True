using CampaignApp.Domain.Common;
using CampaignApp.Domain.Enums;

namespace CampaignApp.Domain.Entities;

public class QuestConnection : AuditableEntity
{
    public Guid SourceQuestId { get; set; }
    public Quest SourceQuest { get; set; } = null!;
    public Guid TargetQuestId { get; set; }
    public Quest TargetQuest { get; set; } = null!;
    public QuestConnectionType ConnectionType { get; set; }
}
