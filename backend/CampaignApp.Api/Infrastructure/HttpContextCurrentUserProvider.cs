using System.Security.Claims;
using CampaignApp.Application.Interfaces;

namespace CampaignApp.Api.Infrastructure;

/// <summary>
/// Resolves the current user's id from the authenticated request's cookie
/// (see Program.cs's cookie authentication setup). Every controller sits
/// behind the global authorization fallback policy by default, so the auth
/// middleware already rejects an unauthenticated request with 401 before
/// any service method - and therefore this method - ever runs; the throw
/// below is a defensive guard against that invariant being broken, not a
/// real control-flow path.
/// </summary>
public class HttpContextCurrentUserProvider : ICurrentUserProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextCurrentUserProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetCurrentUserId()
    {
        var claim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (claim is null || !Guid.TryParse(claim, out var userId))
        {
            throw new InvalidOperationException("No authenticated user is available.");
        }

        return userId;
    }
}
