using CampaignApp.Application.DTOs;

namespace CampaignApp.Application.Services;

public enum LoginOutcome
{
    Success,
    InvalidCredentials,
}

public record LoginResult(LoginOutcome Outcome, UserDto? User);
