using CampaignApp.Domain.Common;

namespace CampaignApp.Domain.Entities;

public class QuestGraphPosition : AuditableEntity
{
    public Guid QuestId { get; set; }
    public Quest Quest { get; set; } = null!;
    public double X { get; set; }
    public double Y { get; set; }
}
