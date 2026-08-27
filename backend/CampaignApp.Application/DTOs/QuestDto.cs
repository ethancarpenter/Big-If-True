using CampaignApp.Domain.Enums;

namespace CampaignApp.Application.DTOs;

public class QuestDto
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public QuestStatus Status { get; set; }
    public QuestType QuestType { get; set; }
    public int? RecommendedLevelMin { get; set; }
    public int? RecommendedLevelMax { get; set; }
    public string? DmNotes { get; set; }
    public List<QuestObjectiveDto> Objectives { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
