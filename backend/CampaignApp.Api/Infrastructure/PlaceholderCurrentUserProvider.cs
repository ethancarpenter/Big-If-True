using CampaignApp.Application.Interfaces;

namespace CampaignApp.Api.Infrastructure;

/// <summary>
/// Milestone 2 placeholder: no authentication exists yet, so every request
/// is treated as the same fixed development user. Replace this
/// implementation (only) once real authentication resolves the current
/// user from the request.
/// </summary>
public class PlaceholderCurrentUserProvider : ICurrentUserProvider
{
    public static readonly Guid DevelopmentUserId = new("00000000-0000-0000-0000-000000000001");

    public Guid GetCurrentUserId() => DevelopmentUserId;
}
