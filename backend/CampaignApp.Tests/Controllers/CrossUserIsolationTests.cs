using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CampaignApp.Application.DTOs;
using CampaignApp.Domain.Enums;

namespace CampaignApp.Tests.Controllers;

/// <summary>
/// Registers two genuinely distinct real users (through the real
/// register/login/cookie flow, not FakeCurrentUserProvider swapping) and
/// confirms one cannot reach the other's data by manually changing IDs.
/// This is a representative smoke test proving the real auth wiring
/// resolves correctly into the ownership logic every entity already
/// enforces (proven per-entity by the existing M2-M9 test suites) - not a
/// re-derivation of those tests.
/// </summary>
public class CrossUserIsolationTests : IClassFixture<RealAuthWebApiFactory>
{
    private readonly RealAuthWebApiFactory _factory;

    public CrossUserIsolationTests(RealAuthWebApiFactory factory)
    {
        _factory = factory;
    }

    private static async Task<string> GetCsrfTokenAsync(HttpClient client)
    {
        var response = await client.GetAsync("/api/auth/csrf");
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("token").GetString()!;
    }

    private static async Task<HttpResponseMessage> PostWithCsrfAsync<T>(
        HttpClient client, string url, T body, string csrfToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(body) };
        request.Headers.Add("X-CSRF-TOKEN", csrfToken);
        return await client.SendAsync(request);
    }

    private async Task<(HttpClient Client, string CsrfToken)> RegisterUserAsync(string email)
    {
        var client = _factory.CreateClient();
        var csrfToken = await GetCsrfTokenAsync(client);
        var response = await PostWithCsrfAsync(
            client, "/api/auth/register",
            new RegisterRequestDto { Email = email, Password = "CorrectHorseBattery1" },
            csrfToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        // The antiforgery token fetched above was issued while anonymous;
        // registering just signed this client in, and the default antiforgery
        // token generator binds tokens to the request's identity at issue
        // time. Refetch for the now-authenticated identity.
        csrfToken = await GetCsrfTokenAsync(client);
        return (client, csrfToken);
    }

    [Fact]
    public async Task UserB_CannotReadUserAsCampaignOrQuest_ByGuessingIds()
    {
        var (clientA, csrfA) = await RegisterUserAsync("isolation-a@example.com");
        var (clientB, _) = await RegisterUserAsync("isolation-b@example.com");

        var campaignResponse = await PostWithCsrfAsync(
            clientA, "/api/campaigns", new CampaignRequestDto { Name = "User A's Campaign" }, csrfA);
        var campaign = await campaignResponse.Content.ReadFromJsonAsync<CampaignDto>();

        var questResponse = await PostWithCsrfAsync(
            clientA, $"/api/campaigns/{campaign!.Id}/quests",
            new QuestRequestDto { Name = "User A's Quest", Status = QuestStatus.Planned, QuestType = QuestType.MainQuest },
            csrfA);
        var quest = await questResponse.Content.ReadFromJsonAsync<QuestDto>(TestJsonOptions.Default);

        var campaignAsB = await clientB.GetAsync($"/api/campaigns/{campaign.Id}");
        Assert.Equal(HttpStatusCode.NotFound, campaignAsB.StatusCode);

        var questAsB = await clientB.GetAsync($"/api/quests/{quest!.Id}");
        Assert.Equal(HttpStatusCode.NotFound, questAsB.StatusCode);

        var questListAsB = await clientB.GetAsync($"/api/campaigns/{campaign.Id}/quests");
        Assert.Equal(HttpStatusCode.NotFound, questListAsB.StatusCode);
    }

    [Fact]
    public async Task UserB_CannotCreateRelationshipReferencingUserAsQuests()
    {
        var (clientA, csrfA) = await RegisterUserAsync("isolation-rel-a@example.com");
        var (clientB, csrfB) = await RegisterUserAsync("isolation-rel-b@example.com");

        var campaignAResponse = await PostWithCsrfAsync(
            clientA, "/api/campaigns", new CampaignRequestDto { Name = "User A's Campaign" }, csrfA);
        var campaignA = await campaignAResponse.Content.ReadFromJsonAsync<CampaignDto>();

        var questA1Response = await PostWithCsrfAsync(
            clientA, $"/api/campaigns/{campaignA!.Id}/quests",
            new QuestRequestDto { Name = "Quest A1", Status = QuestStatus.Planned, QuestType = QuestType.MainQuest },
            csrfA);
        var questA1 = await questA1Response.Content.ReadFromJsonAsync<QuestDto>(TestJsonOptions.Default);

        var questA2Response = await PostWithCsrfAsync(
            clientA, $"/api/campaigns/{campaignA.Id}/quests",
            new QuestRequestDto { Name = "Quest A2", Status = QuestStatus.Planned, QuestType = QuestType.MainQuest },
            csrfA);
        var questA2 = await questA2Response.Content.ReadFromJsonAsync<QuestDto>(TestJsonOptions.Default);

        // User B has their own campaign, but tries to link User A's quests through it.
        var campaignBResponse = await PostWithCsrfAsync(
            clientB, "/api/campaigns", new CampaignRequestDto { Name = "User B's Campaign" }, csrfB);
        var campaignB = await campaignBResponse.Content.ReadFromJsonAsync<CampaignDto>();

        var forgedConnectionResponse = await PostWithCsrfAsync(
            clientB, $"/api/campaigns/{campaignB!.Id}/quest-connections",
            new QuestConnectionCreateRequestDto
            {
                SourceQuestId = questA1!.Id,
                TargetQuestId = questA2!.Id,
                ConnectionType = QuestConnectionType.Unlocks,
            },
            csrfB);

        Assert.Equal(HttpStatusCode.NotFound, forgedConnectionResponse.StatusCode);

        // And directly through User A's own campaign id, still as User B.
        var forgedConnectionViaCampaignA = await PostWithCsrfAsync(
            clientB, $"/api/campaigns/{campaignA.Id}/quest-connections",
            new QuestConnectionCreateRequestDto
            {
                SourceQuestId = questA1.Id,
                TargetQuestId = questA2.Id,
                ConnectionType = QuestConnectionType.Unlocks,
            },
            csrfB);

        Assert.Equal(HttpStatusCode.NotFound, forgedConnectionViaCampaignA.StatusCode);
    }
}
