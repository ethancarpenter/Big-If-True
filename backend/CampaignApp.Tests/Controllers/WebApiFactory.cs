using CampaignApp.Application.Interfaces;
using CampaignApp.Infrastructure.Persistence;
using CampaignApp.Tests.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CampaignApp.Tests.Controllers;

/// <summary>
/// Boots the real Api pipeline (Program.cs, DI, controllers) against an
/// in-memory database and a fixed test user, so ownership enforcement can
/// be verified through the actual HTTP pipeline rather than just the
/// service layer. Shared across every controller's HTTP-level tests.
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
        });
    }
}
