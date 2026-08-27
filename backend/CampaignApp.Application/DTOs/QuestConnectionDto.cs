using CampaignApp.Domain.Enums;

namespace CampaignApp.Application.DTOs;

public class QuestConnectionDto
{
    public Guid Id { get; set; }
    public Guid SourceQuestId { get; set; }
    public string SourceQuestName { get; set; } = string.Empty;
    public Guid TargetQuestId { get; set; }
    public string TargetQuestName { get; set; } = string.Empty;
    public QuestConnectionType ConnectionType { get; set; }
    public DateTime CreatedAt { get; set; }
}
