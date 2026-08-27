using System.ComponentModel.DataAnnotations;

namespace CampaignApp.Application.DTOs;

public class CityRequestDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [StringLength(100)]
    public string? Population { get; set; }

    [StringLength(200)]
    public string? Government { get; set; }

    [StringLength(200)]
    public string? Region { get; set; }

    [StringLength(100)]
    public string? Alignment { get; set; }
}
