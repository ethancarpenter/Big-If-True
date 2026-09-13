using System.Security.Claims;
using CampaignApp.Api.Infrastructure;
using CampaignApp.Application.DTOs;
using CampaignApp.Application.Services;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampaignApp.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IAntiforgery _antiforgery;
    private readonly IDemoLoginResetService _demoLoginReset;

    public AuthController(IAuthService authService, IAntiforgery antiforgery, IDemoLoginResetService demoLoginReset)
    {
        _authService = authService;
        _antiforgery = antiforgery;
        _demoLoginReset = demoLoginReset;
    }

    /// <summary>
    /// Issues a fresh antiforgery cookie/request-token pair. Anonymous so the
    /// frontend can call it before the user has ever logged in - register and
    /// login are themselves CSRF-protected POSTs, so a token has to exist
    /// before either of them can succeed.
    /// </summary>
    [HttpGet("csrf")]
    [AllowAnonymous]
    public IActionResult GetCsrfToken()
    {
        var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
        return Ok(new { token = tokens.RequestToken });
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<UserDto>> Register([FromBody] RegisterRequestDto request)
    {
        var result = await _authService.RegisterAsync(request);
        if (result.Outcome == RegisterOutcome.DuplicateEmail)
        {
            return Conflict();
        }

        await SignInAsync(result.User!);
        return StatusCode(StatusCodes.Status201Created, result.User);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<UserDto>> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);
        if (result.Outcome == LoginOutcome.InvalidCredentials)
        {
            return Unauthorized();
        }

        // Only reached after credentials are already validated - a failed
        // login attempt never reaches, and never triggers, a demo reset.
        // For every account except the reserved demo user (identified by
        // id, not email) with DemoSeed:ResetOnLogin enabled, this is a
        // no-op. On reset failure, fail the login cleanly rather than sign
        // the caller into partially reset demo data.
        var readyToSignIn = await _demoLoginReset.ResetIfDemoAccountAsync(result.User!.Id);
        if (!readyToSignIn)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        await SignInAsync(result.User!);
        return Ok(result.User);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }

    /// <summary>
    /// Sits behind the default global authorization policy like every other
    /// endpoint - no [AllowAnonymous] here. An unauthenticated call already
    /// gets a clean 401 from the auth middleware before this action runs,
    /// which is exactly the signal the frontend needs to know "not logged in".
    /// </summary>
    [HttpGet("me")]
    public ActionResult<UserDto> Me()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);
        return Ok(new UserDto { Id = Guid.Parse(id!), Email = email! });
    }

    private async Task SignInAsync(UserDto user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
    }
}
