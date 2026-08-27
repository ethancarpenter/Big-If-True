namespace CampaignApp.Application.DTOs;

public class QuestObjectiveDto
{
    public Guid Id { get; set; }
    public Guid QuestId { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public int SortOrder { get; set; }
}
