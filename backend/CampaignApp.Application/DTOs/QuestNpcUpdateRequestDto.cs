using System.ComponentModel.DataAnnotations;
using CampaignApp.Domain.Enums;

namespace CampaignApp.Application.DTOs;

/// <summary>No NpcId - it's one of the two keys that define the relationship, so it's immutable after creation.</summary>
public class QuestNpcUpdateRequestDto
{
    [Required]
    public QuestNpcRole? Role { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}
