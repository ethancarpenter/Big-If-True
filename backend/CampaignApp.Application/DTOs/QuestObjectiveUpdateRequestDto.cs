using System.ComponentModel.DataAnnotations;

namespace CampaignApp.Application.DTOs;

public class QuestObjectiveUpdateRequestDto
{
    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    public bool IsCompleted { get; set; }
}
