using System.ComponentModel.DataAnnotations;
using CampaignApp.Domain.Enums;

namespace CampaignApp.Application.DTOs;

public class LocationRequestDto
{
    public Guid CityId { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public LocationType? Type { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    [StringLength(2000)]
    public string? DmNotes { get; set; }
}
