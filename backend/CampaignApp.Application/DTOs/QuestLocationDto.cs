using CampaignApp.Domain.Enums;

namespace CampaignApp.Application.DTOs;

public class QuestLocationDto
{
    public Guid Id { get; set; }
    public Guid QuestId { get; set; }
    public string QuestName { get; set; } = string.Empty;
    public Guid LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public string CityName { get; set; } = string.Empty;
    public QuestLocationRole Role { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
