using System.ComponentModel.DataAnnotations;

namespace CampaignApp.Application.DTOs;

/// <summary>The complete ordered list of this quest's objective ids in their new order.</summary>
public class QuestObjectiveReorderRequestDto
{
    [Required]
    public List<Guid> ObjectiveIds { get; set; } = [];
}
