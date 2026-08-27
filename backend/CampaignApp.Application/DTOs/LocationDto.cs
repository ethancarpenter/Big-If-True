using CampaignApp.Domain.Enums;

namespace CampaignApp.Application.DTOs;

public class LocationDto
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public Guid CityId { get; set; }
    public string CityName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public LocationType Type { get; set; }
    public string? Description { get; set; }
    public string? DmNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
