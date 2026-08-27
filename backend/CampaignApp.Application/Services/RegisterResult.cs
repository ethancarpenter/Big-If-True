using CampaignApp.Application.DTOs;

namespace CampaignApp.Application.Services;

public enum RegisterOutcome
{
    Success,
    DuplicateEmail,
}

public record RegisterResult(RegisterOutcome Outcome, UserDto? User);
