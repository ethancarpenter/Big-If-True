using CampaignApp.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CampaignApp.Api.Infrastructure;

public interface IDemoLoginResetService
{
    /// <summary>
    /// Called from AuthController.Login AFTER credentials have already been
    /// validated successfully. Resets the demo account's data if, and only
    /// if, the authenticated user is the reserved demo user AND
    /// DemoSeed:ResetOnLogin is explicitly enabled - never based on email,
    /// never for any other account, never on a failed login attempt (the
    /// caller only reaches this after a successful LoginAsync).
    ///
    /// Returns true when it is safe to proceed with issuing the auth cookie
    /// (nothing needed resetting, or the reset succeeded); false when the
    /// reset was attempted and failed, in which case the caller must fail
    /// the login instead of signing the user into partially reset data.
    /// </summary>
    Task<bool> ResetIfDemoAccountAsync(Guid authenticatedUserId);
}

public class DemoLoginResetService : IDemoLoginResetService
{
    private readonly AppDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DemoLoginResetService> _logger;

    public DemoLoginResetService(AppDbContext dbContext, IConfiguration configuration, ILogger<DemoLoginResetService> logger)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> ResetIfDemoAccountAsync(Guid authenticatedUserId)
    {
        if (authenticatedUserId != DemoDataSeeder.DemoUserId)
        {
            return true;
        }

        if (!_configuration.GetValue<bool>("DemoSeed:ResetOnLogin"))
        {
            return true;
        }

        try
        {
            await DemoDataSeeder.ResetAsync(_dbContext);
            return true;
        }
        catch (Exception ex)
        {
            // Nothing in a reset ever touches a password or other secret,
            // so there is nothing sensitive to redact here - but the
            // exception message/stack trace is still all that's logged,
            // never any request data.
            _logger.LogError(ex, "Demo account reset-on-login failed; failing the login instead of signing in over partially reset data.");
            return false;
        }
    }
}
