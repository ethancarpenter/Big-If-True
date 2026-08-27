using System.ComponentModel.DataAnnotations;
using CampaignApp.Domain.Enums;

namespace CampaignApp.Application.DTOs;

public class QuestNpcCreateRequestDto
{
    [Required]
    public Guid NpcId { get; set; }

    [Required]
    public QuestNpcRole? Role { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}
