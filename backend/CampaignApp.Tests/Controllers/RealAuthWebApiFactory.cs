using CampaignApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CampaignApp.Tests.Controllers;

/// <summary>
/// Boots the real Api pipeline against an in-memory database, with NO
/// bypass of authentication, authorization, or CSRF - unlike WebApiFactory,
/// which substitutes a fake current-user provider and neutralizes both
/// global policies for the pre-existing ownership test suites. This factory
/// is for the auth-specific tests that need to prove the real
/// HttpContextCurrentUserProvider, cookie middleware, and antiforgery
/// validation actually work end-to-end.
/// </summary>
public class RealAuthWebApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    // WebApplicationFactory defaults the host to the Development environment
    // (matching WebApiFactory's existing behavior, and needed for
    // appsettings.Development.json's connection string - AddHealthChecks
    // reads it eagerly during startup even though InMemory replaces the
    // actual DbContext below). That means Program.cs's Development-only
    // dev@local.test seeding step *does* run once when this factory's host
    // starts, harmlessly adding one extra row nothing here queries for -
    // every test in this file uses its own distinct email, so it never
    // collides with "dev@local.test".
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
        });
    }
}
