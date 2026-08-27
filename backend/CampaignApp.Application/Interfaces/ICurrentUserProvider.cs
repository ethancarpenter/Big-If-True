namespace CampaignApp.Application.Interfaces;

/// <summary>
/// Resolves the id of the user making the current request.
/// The Milestone 2 implementation is a placeholder that returns a fixed
/// development user id — no authentication exists yet. Every consumer of
/// this interface should be written as if it will one day return a real
/// authenticated user id, so swapping the implementation later requires
/// no changes here.
/// </summary>
public interface ICurrentUserProvider
{
    Guid GetCurrentUserId();
}
