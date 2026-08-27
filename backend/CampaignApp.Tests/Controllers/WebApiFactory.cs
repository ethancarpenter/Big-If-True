using CampaignApp.Api.Infrastructure;
using CampaignApp.Application.Interfaces;
using CampaignApp.Infrastructure.Persistence;
using CampaignApp.Tests.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CampaignApp.Tests.Controllers;

/// <summary>
/// Boots the real Api pipeline (Program.cs, DI, controllers) against an
/// in-memory database and a fixed test user, so ownership enforcement can
/// be verified through the actual HTTP pipeline rather than just the
/// service layer. Shared across every controller's HTTP-level tests.
///
/// Since Milestone 10, Program.cs registers a global "every endpoint
/// requires authentication" fallback policy and a global antiforgery
/// filter on every unsafe-verb request. Neither is what these existing
/// ownership tests are about - they predate real auth and exercise
/// ownership logic purely through the substituted ICurrentUserProvider -
/// so both are neutralized here. The real cookie/CSRF pipeline is
/// exercised separately by RealAuthWebApiFactory.
/// </summary>
public class WebApiFactory : WebApplicationFactory<Program>
{
    public Guid TestUserId { get; } = Guid.NewGuid();

    private readonly string _databaseName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (dbContextDescriptor is not null)
            {
                services.Remove(dbContextDescriptor);
            }

            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(_databaseName));

            var currentUserDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(ICurrentUserProvider));
            if (currentUserDescriptor is not null)
            {
                services.Remove(currentUserDescriptor);
            }

            services.AddScoped<ICurrentUserProvider>(_ => new FakeCurrentUserProvider { UserId = TestUserId });

            services.AddAuthorization(options => options.FallbackPolicy = null);

            services.Configure<MvcOptions>(options =>
            {
                var antiforgeryFilter = options.Filters
                    .OfType<AntiforgeryValidationFilter>()
                    .FirstOrDefault();
                if (antiforgeryFilter is not null)
                {
                    options.Filters.Remove(antiforgeryFilter);
                }
            });
        });
    }
}
