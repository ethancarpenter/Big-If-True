using System.ComponentModel.DataAnnotations;
using CampaignApp.Domain.Enums;

namespace CampaignApp.Application.DTOs;

public class QuestLocationCreateRequestDto
{
    [Required]
    public Guid LocationId { get; set; }

    [Required]
    public QuestLocationRole? Role { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}
