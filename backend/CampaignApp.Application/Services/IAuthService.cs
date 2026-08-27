using CampaignApp.Application.DTOs;

namespace CampaignApp.Application.Services;

public interface IAuthService
{
    Task<RegisterResult> RegisterAsync(RegisterRequestDto request);
    Task<LoginResult> LoginAsync(LoginRequestDto request);
}
