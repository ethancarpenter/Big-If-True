using System.ComponentModel.DataAnnotations;

namespace CampaignApp.Application.DTOs;

public class CampaignRequestDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [StringLength(500)]
    [Url]
    public string? CoverImageUrl { get; set; }
}
