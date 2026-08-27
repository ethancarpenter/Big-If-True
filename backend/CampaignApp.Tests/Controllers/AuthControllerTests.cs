using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CampaignApp.Application.DTOs;

namespace CampaignApp.Tests.Controllers;

public class AuthControllerTests : IClassFixture<RealAuthWebApiFactory>
{
    private readonly RealAuthWebApiFactory _factory;

    public AuthControllerTests(RealAuthWebApiFactory factory)
    {
        _factory = factory;
    }

    private static async Task<string> GetCsrfTokenAsync(HttpClient client)
    {
        var response = await client.GetAsync("/api/auth/csrf");
        var raw = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"GET /api/auth/csrf returned {(int)response.StatusCode}: {raw}");
        }
        var body = JsonSerializer.Deserialize<JsonElement>(raw);
        return body.GetProperty("token").GetString()!;
    }

    private static async Task<HttpResponseMessage> PostWithCsrfAsync<T>(
        HttpClient client, string url, T body, string csrfToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(body) };
        request.Headers.Add("X-CSRF-TOKEN", csrfToken);
        return await client.SendAsync(request);
    }

    /// <summary>
    /// ASP.NET Core's default antiforgery token generator binds the token to
    /// the request's identity at issue time - a token fetched while
    /// anonymous does not validate for a request made after signing in (or
    /// after signing out). This mirrors the frontend's own design: the
    /// cached CSRF token is invalidated after register/login/logout, and the
    /// next mutation fetches a fresh one for the new identity. Every helper
    /// below that changes identity returns the freshly-refetched token.
    /// </summary>
    private static async Task<string> RegisterAsync(HttpClient client, string email, string password, string csrfToken)
    {
        var response = await PostWithCsrfAsync(
            client, "/api/auth/register", new RegisterRequestDto { Email = email, Password = password }, csrfToken);
        response.EnsureSuccessStatusCode();
        return await GetCsrfTokenAsync(client);
    }

    [Fact]
    public async Task Register_ReturnsCreated_AndSetsAuthCookie()
    {
        var client = _factory.CreateClient();
        var csrfToken = await GetCsrfTokenAsync(client);

        var response = await PostWithCsrfAsync(
            client, "/api/auth/register",
            new RegisterRequestDto { Email = "dm1@example.com", Password = "CorrectHorseBattery1" },
            csrfToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        Assert.Equal("dm1@example.com", user!.Email);
        Assert.Contains(response.Headers, h => h.Key == "Set-Cookie");
    }

    [Fact]
    public async Task Register_ReturnsConflict_ForDuplicateEmail()
    {
        var client = _factory.CreateClient();
        var csrfToken = await GetCsrfTokenAsync(client);
        csrfToken = await RegisterAsync(client, "dupe@example.com", "CorrectHorseBattery1", csrfToken);

        var response = await PostWithCsrfAsync(
            client, "/api/auth/register",
            new RegisterRequestDto { Email = "dupe@example.com", Password = "SomethingElse1" },
            csrfToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_ForPasswordShorterThanMinimum()
    {
        var client = _factory.CreateClient();
        var csrfToken = await GetCsrfTokenAsync(client);

        var response = await PostWithCsrfAsync(
            client, "/api/auth/register",
            new RegisterRequestDto { Email = "short@example.com", Password = "short1" },
            csrfToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_ReturnsOk_ForValidCredentials()
    {
        var client = _factory.CreateClient();
        var csrfToken = await GetCsrfTokenAsync(client);
        csrfToken = await RegisterAsync(client, "login-ok@example.com", "CorrectHorseBattery1", csrfToken);

        var logoutResponse = await PostWithCsrfAsync(client, "/api/auth/logout", new { }, csrfToken);
        logoutResponse.EnsureSuccessStatusCode();
        csrfToken = await GetCsrfTokenAsync(client);

        var response = await PostWithCsrfAsync(
            client, "/api/auth/login",
            new LoginRequestDto { Email = "login-ok@example.com", Password = "CorrectHorseBattery1" },
            csrfToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_ForWrongPassword()
    {
        var client = _factory.CreateClient();
        var csrfToken = await GetCsrfTokenAsync(client);
        csrfToken = await RegisterAsync(client, "wrongpw@example.com", "CorrectHorseBattery1", csrfToken);

        var logoutResponse = await PostWithCsrfAsync(client, "/api/auth/logout", new { }, csrfToken);
        logoutResponse.EnsureSuccessStatusCode();
        csrfToken = await GetCsrfTokenAsync(client);

        var response = await PostWithCsrfAsync(
            client, "/api/auth/login",
            new LoginRequestDto { Email = "wrongpw@example.com", Password = "WrongPassword1" },
            csrfToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Logout_ClearsSession_SubsequentProtectedCallReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var csrfToken = await GetCsrfTokenAsync(client);
        csrfToken = await RegisterAsync(client, "logout-me@example.com", "CorrectHorseBattery1", csrfToken);

        var meBeforeLogout = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.OK, meBeforeLogout.StatusCode);

        var logoutResponse = await PostWithCsrfAsync(client, "/api/auth/logout", new { }, csrfToken);
        Assert.Equal(HttpStatusCode.NoContent, logoutResponse.StatusCode);

        var meAfterLogout = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, meAfterLogout.StatusCode);
    }

    [Fact]
    public async Task UnauthenticatedAccess_ToProtectedEndpoint_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/campaigns");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Mutation_WithoutCsrfHeader_IsRejected_EvenWhenAuthenticated()
    {
        var client = _factory.CreateClient();
        var csrfToken = await GetCsrfTokenAsync(client);
        await RegisterAsync(client, "csrf-check@example.com", "CorrectHorseBattery1", csrfToken);

        // Authenticated (cookie present via the HttpClient's cookie jar), but
        // no X-CSRF-TOKEN header on this particular request.
        var response = await client.PostAsJsonAsync("/api/campaigns", new CampaignRequestDto { Name = "Should Fail" });

        Assert.NotEqual(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Register_ThenAuthenticatedMutationWithCsrfToken_Succeeds()
    {
        var client = _factory.CreateClient();
        var csrfToken = await GetCsrfTokenAsync(client);
        csrfToken = await RegisterAsync(client, "full-flow@example.com", "CorrectHorseBattery1", csrfToken);

        var response = await PostWithCsrfAsync(
            client, "/api/campaigns",
            new CampaignRequestDto { Name = "My First Campaign" },
            csrfToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var campaign = await response.Content.ReadFromJsonAsync<CampaignDto>();
        Assert.Equal("My First Campaign", campaign!.Name);
    }
}
