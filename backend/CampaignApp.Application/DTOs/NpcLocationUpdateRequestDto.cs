using System.ComponentModel.DataAnnotations;
using CampaignApp.Domain.Enums;

namespace CampaignApp.Application.DTOs;

/// <summary>
/// No LocationId - it's one of the two keys that define the relationship,
/// so it's immutable after creation (see NpcLocationService.UpdateAsync).
/// To "move" a relationship, delete and re-add it instead.
/// </summary>
public class NpcLocationUpdateRequestDto
{
    [Required]
    public NpcLocationRelationshipType? RelationshipType { get; set; }

    public bool IsPrimary { get; set; }
}
