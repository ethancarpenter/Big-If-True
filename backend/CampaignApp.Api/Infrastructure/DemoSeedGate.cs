namespace CampaignApp.Api.Infrastructure;

/// <summary>
/// Pure decision logic for whether/how Program.cs should run demo account
/// seeding outside Development, kept separate purely so it can be unit
/// tested without spinning up a real host - a minimal-hosting Program.cs
/// reads configuration and IWebHostEnvironment before
/// WebApplicationFactory's test overrides can apply, so exercising this
/// gate through a real host would be unreliable. See DemoDataSeeder for
/// what happens once seeding is decided to run.
///
/// Development's own unconditional demo seeding (see Program.cs) never
/// goes through this gate - it always seeds DemoDataSeeder's development
/// defaults directly, regardless of DemoSeed:* configuration.
/// </summary>
public static class DemoSeedGate
{
    public enum Decision
    {
        /// <summary>DemoSeed:Enabled was false (or absent) - do nothing.</summary>
        Skip,

        /// <summary>Enabled with a usable email and password - seed the demo account.</summary>
        Seed,

        /// <summary>Enabled, but email and/or password is missing/blank - fail safe, seed nothing.</summary>
        MisconfiguredSkip,
    }

    public readonly record struct Result(Decision Decision, string? Email, string? Password, string? Warning);

    /// <summary>
    /// Never includes the password in <see cref="Result.Warning"/> or anywhere
    /// else that might reach logs beyond <see cref="Result.Password"/> itself.
    /// </summary>
    public static Result Resolve(bool enabled, string? email, string? password)
    {
        if (!enabled)
        {
            return new Result(Decision.Skip, null, null, null);
        }

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return new Result(
                Decision.MisconfiguredSkip,
                null,
                null,
                "DemoSeed:Enabled is true, but DemoSeed:Email and/or DemoSeed:Password is missing or blank. " +
                "Skipping demo account seeding - no partial account will be created.");
        }

        return new Result(Decision.Seed, email, password, null);
    }
}
