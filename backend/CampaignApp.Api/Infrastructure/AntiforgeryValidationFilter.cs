using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CampaignApp.Api.Infrastructure;

/// <summary>
/// Global CSRF defense for every unsafe-verb (POST/PUT/PATCH/DELETE) request,
/// registered once in Program.cs rather than attributed per-controller.
///
/// Deliberately not using ASP.NET Core's built-in
/// [AutoValidateAntiforgeryToken] here: that attribute resolves
/// AutoValidateAntiforgeryTokenAuthorizationFilter from DI, which is only
/// registered by AddControllersWithViews()/AddRazorPages() (the
/// "ViewFeatures" MVC services) - this app is a pure AddControllers() API
/// with no views, so that attribute throws a DI resolution error at
/// runtime. Calling IAntiforgery.ValidateRequestAsync directly avoids that
/// dependency entirely and is the documented approach for API-only
/// projects.
/// </summary>
public class AntiforgeryValidationFilter : IAsyncAuthorizationFilter
{
    private static readonly HashSet<string> SafeMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        HttpMethods.Get,
        HttpMethods.Head,
        HttpMethods.Options,
        HttpMethods.Trace,
    };

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (SafeMethods.Contains(context.HttpContext.Request.Method))
        {
            return;
        }

        var antiforgery = context.HttpContext.RequestServices.GetRequiredService<IAntiforgery>();
        try
        {
            await antiforgery.ValidateRequestAsync(context.HttpContext);
        }
        catch (AntiforgeryValidationException)
        {
            context.Result = new BadRequestResult();
        }
    }
}
