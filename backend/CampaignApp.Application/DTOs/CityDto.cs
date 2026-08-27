namespace CampaignApp.Application.DTOs;

public class CityDto
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Population { get; set; }
    public string? Government { get; set; }
    public string? Region { get; set; }
    public string? Alignment { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
