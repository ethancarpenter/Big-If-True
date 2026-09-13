using CampaignApp.Api.Infrastructure;
using CampaignApp.Domain.Entities;
using CampaignApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace CampaignApp.Tests.Infrastructure;

public class DemoLoginResetServiceTests
{
    private static readonly Guid DemoUserId = Guid.Parse("00000000-0000-0000-0000-000000000002");

    private static AppDbContext CreateContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;
        return new AppDbContext(options);
    }

    private static IConfiguration CreateConfiguration(bool resetOnLoginEnabled) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DemoSeed:ResetOnLogin"] = resetOnLoginEnabled ? "true" : "false",
            })
            .Build();

    [Fact]
    public async Task ResetIfDemoAccountAsync_NonDemoUser_IsNoOpEvenWhenEnabled()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = CreateContext(dbName);
        var hasher = new PasswordHasher<User>();
        await DemoDataSeeder.SeedAsync(context, hasher, "demo@ethancarpenter.dev", "SuperSecretDemoPass1!");
        var campaignBefore = await context.Campaigns.SingleAsync(c => c.UserId == DemoUserId);

        var service = new DemoLoginResetService(context, CreateConfiguration(resetOnLoginEnabled: true), NullLogger<DemoLoginResetService>.Instance);

        var proceed = await service.ResetIfDemoAccountAsync(Guid.NewGuid());

        Assert.True(proceed);
        var campaignAfter = await context.Campaigns.SingleAsync(c => c.UserId == DemoUserId);
        Assert.Equal(campaignBefore.Id, campaignAfter.Id);
    }

    [Fact]
    public async Task ResetIfDemoAccountAsync_DemoUserWithResetDisabled_IsNoOp()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = CreateContext(dbName);
        var hasher = new PasswordHasher<User>();
        await DemoDataSeeder.SeedAsync(context, hasher, "demo@ethancarpenter.dev", "SuperSecretDemoPass1!");
        var campaign = await context.Campaigns.SingleAsync(c => c.UserId == DemoUserId);
        campaign.Name = "Left exactly as the recruiter set it";
        await context.SaveChangesAsync();

        var service = new DemoLoginResetService(context, CreateConfiguration(resetOnLoginEnabled: false), NullLogger<DemoLoginResetService>.Instance);

        var proceed = await service.ResetIfDemoAccountAsync(DemoUserId);

        Assert.True(proceed);
        var unchanged = await context.Campaigns.SingleAsync(c => c.UserId == DemoUserId);
        Assert.Equal("Left exactly as the recruiter set it", unchanged.Name);
    }

    [Fact]
    public async Task ResetIfDemoAccountAsync_DemoUserWithResetEnabled_ResetsData()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = CreateContext(dbName);
        var hasher = new PasswordHasher<User>();
        await DemoDataSeeder.SeedAsync(context, hasher, "demo@ethancarpenter.dev", "SuperSecretDemoPass1!");
        var campaign = await context.Campaigns.SingleAsync(c => c.UserId == DemoUserId);
        campaign.Name = "Modified before login";
        await context.SaveChangesAsync();

        var service = new DemoLoginResetService(context, CreateConfiguration(resetOnLoginEnabled: true), NullLogger<DemoLoginResetService>.Instance);

        var proceed = await service.ResetIfDemoAccountAsync(DemoUserId);

        Assert.True(proceed);
        var reset = await context.Campaigns.SingleAsync(c => c.UserId == DemoUserId);
        Assert.Equal("The Sunken Lantern", reset.Name);
    }

    [Fact]
    public async Task ResetIfDemoAccountAsync_ResetThrows_ReturnsFalseAndDoesNotPropagate()
    {
        var dbName = Guid.NewGuid().ToString();
        var context = CreateContext(dbName);
        await context.DisposeAsync(); // Guarantees any DB operation below throws.

        var service = new DemoLoginResetService(context, CreateConfiguration(resetOnLoginEnabled: true), NullLogger<DemoLoginResetService>.Instance);

        var proceed = await service.ResetIfDemoAccountAsync(DemoUserId);

        Assert.False(proceed);
    }
}
