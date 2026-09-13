using CampaignApp.Api.Infrastructure;
using CampaignApp.Domain.Entities;
using CampaignApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CampaignApp.Tests.Infrastructure;

public class DemoDataSeederTests
{
    private static readonly Guid DemoUserId = Guid.Parse("00000000-0000-0000-0000-000000000002");

    private static AppDbContext CreateContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task SeedAsync_CreatesDemoUserAndSunkenLanternCampaign()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = CreateContext(dbName);
        var hasher = new PasswordHasher<User>();

        await DemoDataSeeder.SeedAsync(context, hasher, "demo@ethancarpenter.dev", "SuperSecretDemoPass1!");

        var user = await context.Users.SingleAsync(u => u.Id == DemoUserId);
        Assert.Equal("demo@ethancarpenter.dev", user.Email);
        Assert.Equal("DEMO@ETHANCARPENTER.DEV", user.NormalizedEmail);
        Assert.Equal(
            PasswordVerificationResult.Success,
            hasher.VerifyHashedPassword(user, user.PasswordHash, "SuperSecretDemoPass1!"));

        var campaign = await context.Campaigns.SingleAsync(c => c.UserId == DemoUserId);
        Assert.Equal("The Sunken Lantern", campaign.Name);

        // The rest of the sample content hangs off that one campaign.
        Assert.True(await context.Cities.AnyAsync(c => c.CampaignId == campaign.Id));
        Assert.True(await context.Npcs.AnyAsync(n => n.CampaignId == campaign.Id));
        Assert.True(await context.Quests.AnyAsync(q => q.CampaignId == campaign.Id));
    }

    [Fact]
    public async Task SeedAsync_RunTwice_DoesNotDuplicateUserOrCampaign()
    {
        var dbName = Guid.NewGuid().ToString();
        var hasher = new PasswordHasher<User>();

        await using (var first = CreateContext(dbName))
        {
            await DemoDataSeeder.SeedAsync(first, hasher, "demo@ethancarpenter.dev", "SuperSecretDemoPass1!");
        }

        // Simulate a second application startup against the same database.
        await using (var second = CreateContext(dbName))
        {
            await DemoDataSeeder.SeedAsync(second, hasher, "demo@ethancarpenter.dev", "SuperSecretDemoPass1!");
        }

        await using var verify = CreateContext(dbName);
        Assert.Equal(1, await verify.Users.CountAsync(u => u.Id == DemoUserId));
        Assert.Equal(1, await verify.Campaigns.CountAsync(c => c.UserId == DemoUserId));
    }

    [Fact]
    public async Task SeedAsync_DoesNotTouchOtherUsersOrCampaigns()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = CreateContext(dbName);
        var hasher = new PasswordHasher<User>();

        var realUserId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var realUser = new User
        {
            Id = realUserId,
            Email = "recruiter@example.com",
            NormalizedEmail = "RECRUITER@EXAMPLE.COM",
            CreatedAt = now,
            UpdatedAt = now,
        };
        realUser.PasswordHash = hasher.HashPassword(realUser, "RealUserPassword1!");
        context.Users.Add(realUser);
        var realCampaign = new Campaign
        {
            Id = Guid.NewGuid(),
            UserId = realUserId,
            Name = "My Real Campaign",
            CreatedAt = now,
            UpdatedAt = now,
        };
        context.Campaigns.Add(realCampaign);
        await context.SaveChangesAsync();

        await DemoDataSeeder.SeedAsync(context, hasher, "demo@ethancarpenter.dev", "SuperSecretDemoPass1!");

        var untouchedUser = await context.Users.SingleAsync(u => u.Id == realUserId);
        Assert.Equal(
            PasswordVerificationResult.Success,
            hasher.VerifyHashedPassword(untouchedUser, untouchedUser.PasswordHash, "RealUserPassword1!"));
        Assert.Equal(1, await context.Campaigns.CountAsync(c => c.UserId == realUserId));
        Assert.Equal("My Real Campaign", (await context.Campaigns.SingleAsync(c => c.UserId == realUserId)).Name);

        // The demo account owns exactly its own campaign, not the real one.
        var demoCampaigns = await context.Campaigns.Where(c => c.UserId == DemoUserId).ToListAsync();
        Assert.Single(demoCampaigns);
        Assert.Equal("The Sunken Lantern", demoCampaigns[0].Name);
    }

    [Theory]
    [InlineData("", "SomePassword1!")]
    [InlineData("demo@ethancarpenter.dev", "")]
    [InlineData(null, "SomePassword1!")]
    [InlineData("demo@ethancarpenter.dev", null)]
    public async Task SeedAsync_MissingEmailOrPassword_ThrowsAndCreatesNoPartialAccount(string? email, string? password)
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = CreateContext(dbName);
        var hasher = new PasswordHasher<User>();

        await Assert.ThrowsAsync<ArgumentException>(
            () => DemoDataSeeder.SeedAsync(context, hasher, email!, password!));

        Assert.False(await context.Users.AnyAsync(u => u.Id == DemoUserId));
        Assert.False(await context.Campaigns.AnyAsync(c => c.UserId == DemoUserId));
    }

    [Fact]
    public async Task SeedAsync_EmailAlreadyOwnedByAnotherUser_ThrowsAndDoesNotCreateDemoAccount()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = CreateContext(dbName);
        var hasher = new PasswordHasher<User>();

        var now = DateTime.UtcNow;
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "demo@ethancarpenter.dev",
            NormalizedEmail = "DEMO@ETHANCARPENTER.DEV",
            CreatedAt = now,
            UpdatedAt = now,
        };
        existingUser.PasswordHash = hasher.HashPassword(existingUser, "WhoeverRegisteredFirst1!");
        context.Users.Add(existingUser);
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => DemoDataSeeder.SeedAsync(context, hasher, "demo@ethancarpenter.dev", "SuperSecretDemoPass1!"));

        Assert.False(await context.Users.AnyAsync(u => u.Id == DemoUserId));
        // The real account that happened to hold that email is untouched.
        var stillThere = await context.Users.SingleAsync(u => u.Email == "demo@ethancarpenter.dev");
        Assert.Equal(existingUser.Id, stillThere.Id);
    }
}
