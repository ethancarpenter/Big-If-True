using System.ComponentModel.DataAnnotations;
using CampaignApp.Domain.Enums;

namespace CampaignApp.Application.DTOs;

public class NpcLocationCreateRequestDto
{
    [Required]
    public Guid LocationId { get; set; }

    [Required]
    public NpcLocationRelationshipType? RelationshipType { get; set; }

    public bool IsPrimary { get; set; }
}
