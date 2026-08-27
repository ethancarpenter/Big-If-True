using System.ComponentModel.DataAnnotations;

namespace CampaignApp.Application.DTOs;

public class QuestGraphPositionUpdateRequestDto
{
    [Required]
    public double? X { get; set; }

    [Required]
    public double? Y { get; set; }
}
