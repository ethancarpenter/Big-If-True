using CampaignApp.Domain.Enums;

namespace CampaignApp.Application.DTOs;

public class QuestNpcDto
{
    public Guid Id { get; set; }
    public Guid QuestId { get; set; }
    public string QuestName { get; set; } = string.Empty;
    public Guid NpcId { get; set; }
    public string NpcName { get; set; } = string.Empty;
    public QuestNpcRole Role { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
