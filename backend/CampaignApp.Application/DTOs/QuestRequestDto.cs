using System.ComponentModel.DataAnnotations;
using CampaignApp.Domain.Enums;

namespace CampaignApp.Application.DTOs;

public class QuestRequestDto : IValidatableObject
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required]
    public QuestStatus? Status { get; set; }

    [Required]
    public QuestType? QuestType { get; set; }

    [Range(1, 20)]
    public int? RecommendedLevelMin { get; set; }

    [Range(1, 20)]
    public int? RecommendedLevelMax { get; set; }

    [StringLength(2000)]
    public string? DmNotes { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (RecommendedLevelMin.HasValue && RecommendedLevelMax.HasValue && RecommendedLevelMin > RecommendedLevelMax)
        {
            yield return new ValidationResult(
                "RecommendedLevelMin must be less than or equal to RecommendedLevelMax.",
                [nameof(RecommendedLevelMin), nameof(RecommendedLevelMax)]);
        }
    }
}
