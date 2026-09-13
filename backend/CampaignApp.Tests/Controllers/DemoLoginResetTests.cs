using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CampaignApp.Api.Infrastructure;
using CampaignApp.Application.DTOs;
using CampaignApp.Domain.Entities;
using CampaignApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CampaignApp.Tests.Controllers;

/// <summary>
/// Exercises DemoSeed:ResetOnLogin end-to-end through the real
/// /api/auth/login endpoint - real credential validation, real cookie
/// issuance, no bypass. Development's own unconditional startup seeding
/// (see Program.cs) provides the demo@local.test / DemoPassword123!
/// account under the same reserved id production uses, so no extra setup
/// is needed to have a demo account to log into.
/// </summary>
public class DemoLoginResetTests
{
    private static readonly Guid DemoUserId = Guid.Parse("00000000-0000-0000-0000-000000000002");

    private static async Task<string> GetCsrfTokenAsync(HttpClient client)
    {
        var response = await client.GetAsync("/api/auth/csrf");
        var raw = await response.Content.ReadAsStringAsync();
        var body = JsonSerializer.Deserialize<JsonElement>(raw);
        return body.GetProperty("token").GetString()!;
    }

    private static async Task<HttpResponseMessage> LoginAsync(HttpClient client, string email, string password)
    {
        var csrfToken = await GetCsrfTokenAsync(client);
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new LoginRequestDto { Email = email, Password = password }),
        };
        request.Headers.Add("X-CSRF-TOKEN", csrfToken);
        return await client.SendAsync(request);
    }

    private static async Task<HttpResponseMessage> RegisterAsync(HttpClient client, string email, string password)
    {
        var csrfToken = await GetCsrfTokenAsync(client);
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/register")
        {
            Content = JsonContent.Create(new RegisterRequestDto { Email = email, Password = password }),
        };
        request.Headers.Add("X-CSRF-TOKEN", csrfToken);
        return await client.SendAsync(request);
    }

    private static AppDbContext GetDbContext(DemoLoginResetWebApiFactory factory) =>
        factory.Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();

    [Fact]
    public async Task SuccessfulDemoLogin_WithResetEnabled_RestoresModifiedCampaignContent()
    {
        await using var factory = new DemoLoginResetWebApiFactory(resetOnLoginEnabled: true);
        var client = factory.CreateClient();
        var db = GetDbContext(factory);

        var campaign = await db.Campaigns.SingleAsync(c => c.UserId == DemoUserId);
        campaign.Name = "Renamed by a recruiter";
        await db.SaveChangesAsync();

        var response = await LoginAsync(client, DemoDataSeeder.DefaultDevelopmentEmail, DemoDataSeeder.DefaultDevelopmentPassword);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var reset = await db.Campaigns.SingleAsync(c => c.UserId == DemoUserId);
        Assert.Equal("The Sunken Lantern", reset.Name);
    }

    [Fact]
    public async Task SuccessfulDemoLogin_WithResetEnabled_RestoresDeletedEntities()
    {
        await using var factory = new DemoLoginResetWebApiFactory(resetOnLoginEnabled: true);
        var client = factory.CreateClient();
        var db = GetDbContext(factory);

        var campaign = await db.Campaigns.SingleAsync(c => c.UserId == DemoUserId);
        db.Quests.RemoveRange(db.Quests.Where(q => q.CampaignId == campaign.Id));
        await db.SaveChangesAsync();
        Assert.False(await db.Quests.AnyAsync(q => q.CampaignId == campaign.Id));

        var response = await LoginAsync(client, DemoDataSeeder.DefaultDevelopmentEmail, DemoDataSeeder.DefaultDevelopmentPassword);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var restoredCampaign = await db.Campaigns.SingleAsync(c => c.UserId == DemoUserId);
        Assert.True(await db.Quests.AnyAsync(q => q.CampaignId == restoredCampaign.Id && q.Name == "The Missing Keeper"));
    }

    [Fact]
    public async Task SuccessfulDemoLogin_WithResetEnabled_RemovesEntitiesAddedSinceSeeding()
    {
        await using var factory = new DemoLoginResetWebApiFactory(resetOnLoginEnabled: true);
        var client = factory.CreateClient();
        var db = GetDbContext(factory);

        var now = DateTime.UtcNow;
        db.Campaigns.Add(new Campaign { Id = Guid.NewGuid(), UserId = DemoUserId, Name = "A recruiter's extra campaign", CreatedAt = now, UpdatedAt = now });
        await db.SaveChangesAsync();
        Assert.Equal(2, await db.Campaigns.CountAsync(c => c.UserId == DemoUserId));

        var response = await LoginAsync(client, DemoDataSeeder.DefaultDevelopmentEmail, DemoDataSeeder.DefaultDevelopmentPassword);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var campaigns = await db.Campaigns.Where(c => c.UserId == DemoUserId).ToListAsync();
        var onlyCampaign = Assert.Single(campaigns);
        Assert.Equal("The Sunken Lantern", onlyCampaign.Name);
    }

    [Fact]
    public async Task SuccessfulDemoLogin_WithResetDisabled_LeavesDataUnchanged()
    {
        await using var factory = new DemoLoginResetWebApiFactory(resetOnLoginEnabled: false);
        var client = factory.CreateClient();
        var db = GetDbContext(factory);

        var campaign = await db.Campaigns.SingleAsync(c => c.UserId == DemoUserId);
        campaign.Name = "Left exactly as the recruiter set it";
        await db.SaveChangesAsync();

        var response = await LoginAsync(client, DemoDataSeeder.DefaultDevelopmentEmail, DemoDataSeeder.DefaultDevelopmentPassword);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var unchanged = await db.Campaigns.SingleAsync(c => c.UserId == DemoUserId);
        Assert.Equal("Left exactly as the recruiter set it", unchanged.Name);
    }

    [Fact]
    public async Task FailedDemoLogin_WrongPassword_DoesNotResetData()
    {
        await using var factory = new DemoLoginResetWebApiFactory(resetOnLoginEnabled: true);
        var client = factory.CreateClient();
        var db = GetDbContext(factory);

        var campaign = await db.Campaigns.SingleAsync(c => c.UserId == DemoUserId);
        campaign.Name = "Should survive a failed login attempt";
        await db.SaveChangesAsync();

        var response = await LoginAsync(client, DemoDataSeeder.DefaultDevelopmentEmail, "TotallyWrongPassword1!");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var stillThere = await db.Campaigns.SingleAsync(c => c.UserId == DemoUserId);
        Assert.Equal("Should survive a failed login attempt", stillThere.Name);
    }

    [Fact]
    public async Task NormalUserLogin_NeverTriggersReset_AndNeverTouchesDemoOrOwnData()
    {
        await using var factory = new DemoLoginResetWebApiFactory(resetOnLoginEnabled: true);
        var client = factory.CreateClient();
        var db = GetDbContext(factory);

        // Mutate the demo campaign so a false-positive reset would be visible.
        var demoCampaign = await db.Campaigns.SingleAsync(c => c.UserId == DemoUserId);
        demoCampaign.Name = "Must not be touched by a normal user login";
        await db.SaveChangesAsync();

        var registerResponse = await RegisterAsync(client, "recruiter-viewer@example.com", "CorrectHorseBattery1");
        registerResponse.EnsureSuccessStatusCode();
        var normalUser = await registerResponse.Content.ReadFromJsonAsync<UserDto>();

        var csrfToken = await GetCsrfTokenAsync(client);
        var createCampaignRequest = new HttpRequestMessage(HttpMethod.Post, "/api/campaigns")
        {
            Content = JsonContent.Create(new CampaignRequestDto { Name = "My Own Campaign" }),
        };
        createCampaignRequest.Headers.Add("X-CSRF-TOKEN", csrfToken);
        var createResponse = await client.SendAsync(createCampaignRequest);
        createResponse.EnsureSuccessStatusCode();

        var logoutCsrf = await GetCsrfTokenAsync(client);
        var logoutRequest = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout");
        logoutRequest.Headers.Add("X-CSRF-TOKEN", logoutCsrf);
        (await client.SendAsync(logoutRequest)).EnsureSuccessStatusCode();

        var loginResponse = await LoginAsync(client, "recruiter-viewer@example.com", "CorrectHorseBattery1");

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        // The demo account's mutated content is untouched - no reset fired.
        var demoAfter = await db.Campaigns.SingleAsync(c => c.UserId == DemoUserId);
        Assert.Equal("Must not be touched by a normal user login", demoAfter.Name);
        // The normal user's own campaign is untouched too.
        Assert.Equal(1, await db.Campaigns.CountAsync(c => c.UserId == normalUser!.Id));
    }

    [Fact]
    public async Task RepeatedDemoLogins_WithResetEnabled_RemainIdempotent()
    {
        await using var factory = new DemoLoginResetWebApiFactory(resetOnLoginEnabled: true);
        var client = factory.CreateClient();
        var db = GetDbContext(factory);

        var first = await LoginAsync(client, DemoDataSeeder.DefaultDevelopmentEmail, DemoDataSeeder.DefaultDevelopmentPassword);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        var logoutCsrf = await GetCsrfTokenAsync(client);
        var logoutRequest = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout");
        logoutRequest.Headers.Add("X-CSRF-TOKEN", logoutCsrf);
        (await client.SendAsync(logoutRequest)).EnsureSuccessStatusCode();

        var second = await LoginAsync(client, DemoDataSeeder.DefaultDevelopmentEmail, DemoDataSeeder.DefaultDevelopmentPassword);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);

        var campaigns = await db.Campaigns.Where(c => c.UserId == DemoUserId).ToListAsync();
        var onlyCampaign = Assert.Single(campaigns);
        Assert.Equal("The Sunken Lantern", onlyCampaign.Name);
        Assert.Equal(3, await db.Quests.CountAsync(q => q.CampaignId == onlyCampaign.Id));
    }
}
