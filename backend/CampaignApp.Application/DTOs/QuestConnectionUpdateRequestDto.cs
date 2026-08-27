using System.ComponentModel.DataAnnotations;
using CampaignApp.Domain.Enums;

namespace CampaignApp.Application.DTOs;

public class QuestConnectionUpdateRequestDto
{
    [Required]
    public QuestConnectionType? ConnectionType { get; set; }
}
