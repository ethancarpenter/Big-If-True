using System.ComponentModel.DataAnnotations;
using CampaignApp.Domain.Enums;

namespace CampaignApp.Application.DTOs;

public class QuestConnectionCreateRequestDto : IValidatableObject
{
    [Required]
    public Guid SourceQuestId { get; set; }

    [Required]
    public Guid TargetQuestId { get; set; }

    [Required]
    public QuestConnectionType? ConnectionType { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (SourceQuestId == TargetQuestId)
        {
            yield return new ValidationResult(
                "A quest cannot connect to itself.",
                [nameof(SourceQuestId), nameof(TargetQuestId)]);
        }
    }
}
