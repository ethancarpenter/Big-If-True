using CampaignApp.Domain.Enums;

namespace CampaignApp.Application.DTOs;

public class NpcDto
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Species { get; set; }
    public string? Gender { get; set; }
    public int? Age { get; set; }
    public NpcClass? Class { get; set; }
    public Alignment? Alignment { get; set; }
    public string? Occupation { get; set; }
    public string? Disposition { get; set; }
    public string? Description { get; set; }
    public string? DmNotes { get; set; }
    public NpcStatus Status { get; set; }
    public string? PortraitUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
