using System.ComponentModel.DataAnnotations;
using CampaignApp.Domain.Enums;

namespace CampaignApp.Application.DTOs;

public class NpcRequestDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Species { get; set; }

    [StringLength(100)]
    public string? Gender { get; set; }

    [Range(0, 100000)]
    public int? Age { get; set; }

    public NpcClass? Class { get; set; }

    public Alignment? Alignment { get; set; }

    [StringLength(200)]
    public string? Occupation { get; set; }

    [StringLength(100)]
    public string? Disposition { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    [StringLength(2000)]
    public string? DmNotes { get; set; }

    [Required]
    public NpcStatus? Status { get; set; }

    [StringLength(500)]
    [Url]
    public string? PortraitUrl { get; set; }
}
