using System.ComponentModel.DataAnnotations;
using CampaignApp.Domain.Enums;

namespace CampaignApp.Application.DTOs;

/// <summary>No LocationId - it's one of the two keys that define the relationship, so it's immutable after creation.</summary>
public class QuestLocationUpdateRequestDto
{
    [Required]
    public QuestLocationRole? Role { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}
