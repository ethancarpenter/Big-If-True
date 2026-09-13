using CampaignApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CampaignApp.Tests.Controllers;

/// <summary>
/// Like RealAuthWebApiFactory (real pipeline, no auth/CSRF bypass), but
/// configurable per test for DemoSeed:ResetOnLogin so the reset-on-login
/// behavior can be exercised through the actual /api/auth/login endpoint.
/// Stays in the default Development environment - config read at request
/// time (inside DemoLoginResetService, not during Program.cs's top-level
/// statements) applies correctly regardless of environment, and Development
/// conveniently already seeds demo@local.test / DemoPassword123! (same
/// reserved user id as production) unconditionally at startup.
/// </summary>
public class DemoLoginResetWebApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = Guid.NewGuid().ToString();
    private readonly bool _resetOnLoginEnabled;

    public DemoLoginResetWebApiFactory(bool resetOnLoginEnabled)
    {
        _resetOnLoginEnabled = resetOnLoginEnabled;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DemoSeed:ResetOnLogin"] = _resetOnLoginEnabled ? "true" : "false",
            });
        });

        builder.ConfigureServices(services =>
        {
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (dbContextDescriptor is not null)
            {
                services.Remove(dbContextDescriptor);
            }

            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(_databaseName));
        });
    }
}
