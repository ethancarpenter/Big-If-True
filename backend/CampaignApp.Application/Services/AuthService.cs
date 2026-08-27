using CampaignApp.Application.DTOs;
using CampaignApp.Application.Interfaces;
using CampaignApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace CampaignApp.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AuthService(IUserRepository userRepository, IPasswordHasher<User> passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<RegisterResult> RegisterAsync(RegisterRequestDto request)
    {
        var normalizedEmail = Normalize(request.Email);

        var existing = await _userRepository.GetByNormalizedEmailAsync(normalizedEmail);
        if (existing is not null)
        {
            return new RegisterResult(RegisterOutcome.DuplicateEmail, null);
        }

        var now = DateTime.UtcNow;
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.Trim(),
            NormalizedEmail = normalizedEmail,
            CreatedAt = now,
            UpdatedAt = now,
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        return new RegisterResult(RegisterOutcome.Success, ToDto(user));
    }

    public async Task<LoginResult> LoginAsync(LoginRequestDto request)
    {
        var normalizedEmail = Normalize(request.Email);

        var user = await _userRepository.GetByNormalizedEmailAsync(normalizedEmail);
        if (user is null)
        {
            return new LoginResult(LoginOutcome.InvalidCredentials, null);
        }

        var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            return new LoginResult(LoginOutcome.InvalidCredentials, null);
        }

        return new LoginResult(LoginOutcome.Success, ToDto(user));
    }

    private static string Normalize(string email) => email.Trim().ToUpperInvariant();

    private static UserDto ToDto(User user) => new()
    {
        Id = user.Id,
        Email = user.Email,
    };
}
