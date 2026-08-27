using System.Security.Claims;
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

    public AuthController(IAuthService authService, IAntiforgery antiforgery)
    {
        _authService = authService;
        _antiforgery = antiforgery;
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
