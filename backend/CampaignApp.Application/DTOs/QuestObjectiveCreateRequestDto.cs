using System.ComponentModel.DataAnnotations;

namespace CampaignApp.Application.DTOs;

/// <summary>No SortOrder - the server always appends new objectives to the end.</summary>
public class QuestObjectiveCreateRequestDto
{
    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
}
