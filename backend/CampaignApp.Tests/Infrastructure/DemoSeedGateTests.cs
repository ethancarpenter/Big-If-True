using CampaignApp.Api.Infrastructure;

namespace CampaignApp.Tests.Infrastructure;

public class DemoSeedGateTests
{
    [Fact]
    public void Resolve_Disabled_Skips()
    {
        var result = DemoSeedGate.Resolve(enabled: false, email: "demo@ethancarpenter.dev", password: "SuperSecretDemoPass1!");

        Assert.Equal(DemoSeedGate.Decision.Skip, result.Decision);
        Assert.Null(result.Email);
        Assert.Null(result.Password);
        Assert.Null(result.Warning);
    }

    [Fact]
    public void Resolve_DisabledEvenWithMissingCredentials_SkipsWithoutWarning()
    {
        // Being off is always a clean no-op, regardless of what else is (or
        // isn't) configured - no warning noise for the common case.
        var result = DemoSeedGate.Resolve(enabled: false, email: null, password: null);

        Assert.Equal(DemoSeedGate.Decision.Skip, result.Decision);
        Assert.Null(result.Warning);
    }

    [Fact]
    public void Resolve_EnabledWithValidCredentials_Seeds()
    {
        var result = DemoSeedGate.Resolve(enabled: true, email: "demo@ethancarpenter.dev", password: "SuperSecretDemoPass1!");

        Assert.Equal(DemoSeedGate.Decision.Seed, result.Decision);
        Assert.Equal("demo@ethancarpenter.dev", result.Email);
        Assert.Equal("SuperSecretDemoPass1!", result.Password);
        Assert.Null(result.Warning);
    }

    [Theory]
    [InlineData(null, "SuperSecretDemoPass1!")]
    [InlineData("demo@ethancarpenter.dev", null)]
    [InlineData("", "SuperSecretDemoPass1!")]
    [InlineData("demo@ethancarpenter.dev", "")]
    [InlineData("   ", "SuperSecretDemoPass1!")]
    public void Resolve_EnabledWithMissingOrBlankCredentials_FailsSafelyWithoutSeeding(string? email, string? password)
    {
        var result = DemoSeedGate.Resolve(enabled: true, email, password);

        Assert.Equal(DemoSeedGate.Decision.MisconfiguredSkip, result.Decision);
        Assert.Null(result.Email);
        Assert.Null(result.Password);
        Assert.NotNull(result.Warning);
        // The warning must never leak the (possibly present) password value.
        Assert.DoesNotContain("SuperSecretDemoPass1!", result.Warning);
    }
}
