using System.ComponentModel.DataAnnotations;

namespace CampaignApp.Application.DTOs;

public class RegisterRequestDto
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    [StringLength(200)]
    public string Password { get; set; } = string.Empty;
}
