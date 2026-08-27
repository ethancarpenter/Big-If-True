using CampaignApp.Domain.Enums;

namespace CampaignApp.Application.DTOs;

public class NpcLocationDto
{
    public Guid Id { get; set; }
    public Guid NpcId { get; set; }
    public string NpcName { get; set; } = string.Empty;
    public Guid LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public string CityName { get; set; } = string.Empty;
    public NpcLocationRelationshipType RelationshipType { get; set; }
    public bool IsPrimary { get; set; }
    public DateTime CreatedAt { get; set; }
}
