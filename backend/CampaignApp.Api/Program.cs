using System.Text.Json.Serialization;
using CampaignApp.Api.Infrastructure;
using CampaignApp.Application.Interfaces;
using CampaignApp.Application.Services;
using CampaignApp.Domain.Entities;
using CampaignApp.Infrastructure.Persistence;
using CampaignApp.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

const string DevCorsPolicy = "DevCorsPolicy";

// Add services to the container.

builder.Services.AddControllers(options =>
    {
        // Global, fail-closed: every unsafe-verb (POST/PUT/PATCH/DELETE) request
        // must carry a valid antiforgery cookie/header pair. GET/HEAD/OPTIONS are
        // unaffected. No per-controller attributes needed.
        options.Filters.Add(new AntiforgeryValidationFilter());
    })
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString!, name: "postgres");

builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "campaignapp.auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        // The frontend (campaigns.ethancarpenter.dev) and this API
        // (api-campaigns.ethancarpenter.dev) are sibling subdomains, so the
        // auth cookie needs an explicit parent domain to be sent cross-origin
        // between them. Left unset in Development so local testing keeps the
        // default host-only cookie.
        var cookieDomain = builder.Configuration["CookieDomain"];
        if (!builder.Environment.IsDevelopment() && !string.IsNullOrWhiteSpace(cookieDomain))
        {
            options.Cookie.Domain = cookieDomain;
        }
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.SlidingExpiration = true;
        // This is a pure JSON API - there's no login page on this server (it
        // lives in the separate Next.js app), so the default "redirect to a
        // login page" behavior would send a fetch() call down a confusing 302
        // instead of a clean status code.
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization(options =>
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "campaignapp.csrf";
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    // Cookie stays HttpOnly (the default) - the frontend gets the request
    // token from the /api/auth/csrf response body, never needs to read the
    // cookie itself.
});

builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.AddScoped<ICampaignRepository, CampaignRepository>();
builder.Services.AddScoped<ICampaignService, CampaignService>();
builder.Services.AddScoped<ICityRepository, CityRepository>();
builder.Services.AddScoped<ICityService, CityService>();
builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<INpcRepository, NpcRepository>();
builder.Services.AddScoped<INpcService, NpcService>();
builder.Services.AddScoped<INpcLocationRepository, NpcLocationRepository>();
builder.Services.AddScoped<INpcLocationService, NpcLocationService>();
builder.Services.AddScoped<IQuestRepository, QuestRepository>();
builder.Services.AddScoped<IQuestService, QuestService>();
builder.Services.AddScoped<IQuestObjectiveRepository, QuestObjectiveRepository>();
builder.Services.AddScoped<IQuestObjectiveService, QuestObjectiveService>();
builder.Services.AddScoped<IQuestNpcRepository, QuestNpcRepository>();
builder.Services.AddScoped<IQuestNpcService, QuestNpcService>();
builder.Services.AddScoped<IQuestLocationRepository, QuestLocationRepository>();
builder.Services.AddScoped<IQuestLocationService, QuestLocationService>();
builder.Services.AddScoped<IQuestConnectionRepository, QuestConnectionRepository>();
builder.Services.AddScoped<IQuestConnectionService, QuestConnectionService>();
builder.Services.AddScoped<IQuestGraphPositionRepository, QuestGraphPositionRepository>();
builder.Services.AddScoped<IQuestGraphPositionService, QuestGraphPositionService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<ICurrentUserProvider, HttpContextCurrentUserProvider>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(DevCorsPolicy, policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// In every non-development environment this API runs behind a TLS-terminating
// reverse proxy (Render's router, itself typically behind Cloudflare). The
// proxy speaks plain HTTP to this process and reports the original request
// scheme in X-Forwarded-Proto. Without honoring that header, every request
// looks like HTTP here: Request.IsHttps is false, so the Secure-flagged auth
// and antiforgery cookies (CookieSecurePolicy.Always outside Development) are
// never issued or accepted, and HttpsRedirection can loop.
//
// What is trusted, and why it is safe:
//   * ONLY X-Forwarded-Proto is consumed. X-Forwarded-For is not (the app
//     makes no security decision on client IP) and X-Forwarded-Host is not
//     (host validation stays with AllowedHosts).
//   * ForwardLimit = 1: only the value appended by the immediate upstream
//     hop is read; anything a client tries to smuggle earlier in the chain
//     is discarded.
//   * KnownProxies / KnownNetworks are cleared rather than pinned. Render
//     assigns its proxy a dynamic internal address and publishes no stable
//     proxy IP or CIDR to trust, so there is nothing meaningful to pin. This
//     is acceptable here because the container's HTTP port is not
//     internet-routable - the only path to it is through Render's edge,
//     which sets X-Forwarded-Proto itself and overwrites any inbound value.
// In Development there is no proxy, so this is left off entirely.
if (!builder.Environment.IsDevelopment())
{
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
        options.ForwardLimit = 1;
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });
}

// Persist the Data Protection key ring in PostgreSQL via EF Core. The key
// ring signs and encrypts the auth and antiforgery cookies; the default
// file-system store lives in the container and is lost on every restart or
// redeploy, which would silently invalidate every active session. Backing it
// with the database makes it survive restarts, redeploys, and running more
// than one instance, with no dependency on a persistent disk. SetApplicationName
// pins the key isolation identifier so it stays stable regardless of hosting.
builder.Services.AddDataProtection()
    .PersistKeysToDbContext<AppDbContext>()
    .SetApplicationName("CampaignApp");

var app = builder.Build();

// Must run before UseHttpsRedirection / UseAuthentication / UseCors so the
// rest of the pipeline sees the corrected scheme. No-op in Development (the
// options above are only registered outside it).
app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

    if (app.Environment.IsDevelopment())
    {
        // Development-only, idempotent: ensure the known dev@local.test /
        // DevPassword123! credential exists and is current, so the campaigns
        // seeded under the legacy placeholder id (see the
        // AddUsersAndCampaignOwnerFk migration) stay reachable in local dev.
        // This is the ONLY place that password is ever established - never in
        // a migration, and structurally unreachable outside Development.
        var devUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        var devUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == devUserId);
        var now = DateTime.UtcNow;
        if (devUser is null)
        {
            devUser = new User
            {
                Id = devUserId,
                Email = "dev@local.test",
                NormalizedEmail = "DEV@LOCAL.TEST",
                CreatedAt = now,
                UpdatedAt = now,
            };
            devUser.PasswordHash = hasher.HashPassword(devUser, "DevPassword123!");
            dbContext.Users.Add(devUser);
        }
        else
        {
            devUser.PasswordHash = hasher.HashPassword(devUser, "DevPassword123!");
            devUser.UpdatedAt = now;
        }

        await dbContext.SaveChangesAsync();

        // Development-only, idempotent: a separate demo@local.test account
        // and self-contained campaign, so the app can be reviewed without
        // depending on or cluttering the dev user's own accumulated data.
        await DemoDataSeeder.SeedAsync(dbContext, hasher, DemoDataSeeder.DefaultDevelopmentEmail, DemoDataSeeder.DefaultDevelopmentPassword);
    }
    else
    {
        // Outside Development, the demo account is opt-in only: it never
        // runs unless DemoSeed:Enabled is explicitly set (Render env var
        // DemoSeed__Enabled=true), and it never touches dev@local.test or
        // any other seeding path. This is also the ONLY place the demo
        // password is established outside Development - it is never
        // hard-coded and never logged. DemoSeedGate.Resolve is the pure,
        // unit-tested decision of whether/how to proceed.
        var gate = DemoSeedGate.Resolve(
            builder.Configuration.GetValue<bool>("DemoSeed:Enabled"),
            builder.Configuration["DemoSeed:Email"],
            builder.Configuration["DemoSeed:Password"]);

        if (gate.Decision == DemoSeedGate.Decision.MisconfiguredSkip)
        {
            scope.ServiceProvider.GetRequiredService<ILogger<Program>>().LogError(gate.Warning);
        }
        else if (gate.Decision == DemoSeedGate.Decision.Seed)
        {
            try
            {
                await DemoDataSeeder.SeedAsync(dbContext, hasher, gate.Email!, gate.Password!);
            }
            catch (Exception ex)
            {
                // A demo-seeding problem (e.g. the configured email already
                // belongs to a real user) must never take down API startup
                // or touch unrelated data - log and continue. The password
                // value itself is never included in this message.
                scope.ServiceProvider.GetRequiredService<ILogger<Program>>()
                    .LogError(ex, "Demo account seeding failed and was skipped.");
            }
        }
    }
}

app.UseHttpsRedirection();

app.UseCors(DevCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/api/health").AllowAnonymous();

app.Run();

// Exposed so CampaignApp.Tests can bootstrap this app via WebApplicationFactory<Program>.
public partial class Program { }
