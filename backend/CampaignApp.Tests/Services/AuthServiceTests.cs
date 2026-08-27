using CampaignApp.Application.DTOs;
using CampaignApp.Application.Services;
using CampaignApp.Domain.Entities;
using CampaignApp.Infrastructure.Persistence;
using CampaignApp.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CampaignApp.Tests.Services;

public class AuthServiceTests
{
    private static AuthService CreateService()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new AppDbContext(options);
        var userRepository = new UserRepository(context);
        var hasher = new PasswordHasher<User>();
        return new AuthService(userRepository, hasher);
    }

    [Fact]
    public async Task RegisterAsync_CreatesUser_OnFirstRegistration()
    {
        var service = CreateService();

        var result = await service.RegisterAsync(new RegisterRequestDto
        {
            Email = "dm@example.com",
            Password = "CorrectHorseBattery1",
        });

        Assert.Equal(RegisterOutcome.Success, result.Outcome);
        Assert.NotNull(result.User);
        Assert.Equal("dm@example.com", result.User!.Email);
        Assert.NotEqual(Guid.Empty, result.User.Id);
    }

    [Fact]
    public async Task RegisterAsync_ReturnsDuplicateEmail_ForSameEmailDifferentCase()
    {
        var service = CreateService();
        await service.RegisterAsync(new RegisterRequestDto { Email = "dm@example.com", Password = "CorrectHorseBattery1" });

        var result = await service.RegisterAsync(new RegisterRequestDto
        {
            Email = "DM@Example.com",
            Password = "AnotherPassword1",
        });

        Assert.Equal(RegisterOutcome.DuplicateEmail, result.Outcome);
        Assert.Null(result.User);
    }

    [Fact]
    public async Task LoginAsync_SucceedsWithCorrectPassword()
    {
        var service = CreateService();
        await service.RegisterAsync(new RegisterRequestDto { Email = "dm@example.com", Password = "CorrectHorseBattery1" });

        var result = await service.LoginAsync(new LoginRequestDto { Email = "dm@example.com", Password = "CorrectHorseBattery1" });

        Assert.Equal(LoginOutcome.Success, result.Outcome);
        Assert.NotNull(result.User);
        Assert.Equal("dm@example.com", result.User!.Email);
    }

    [Fact]
    public async Task LoginAsync_SucceedsCaseInsensitiveOnEmail()
    {
        var service = CreateService();
        await service.RegisterAsync(new RegisterRequestDto { Email = "dm@example.com", Password = "CorrectHorseBattery1" });

        var result = await service.LoginAsync(new LoginRequestDto { Email = "DM@EXAMPLE.COM", Password = "CorrectHorseBattery1" });

        Assert.Equal(LoginOutcome.Success, result.Outcome);
    }

    [Fact]
    public async Task LoginAsync_ReturnsInvalidCredentials_ForUnknownEmail()
    {
        var service = CreateService();

        var result = await service.LoginAsync(new LoginRequestDto { Email = "nobody@example.com", Password = "WhateverPassword1" });

        Assert.Equal(LoginOutcome.InvalidCredentials, result.Outcome);
        Assert.Null(result.User);
    }

    [Fact]
    public async Task LoginAsync_ReturnsInvalidCredentials_ForWrongPassword()
    {
        var service = CreateService();
        await service.RegisterAsync(new RegisterRequestDto { Email = "dm@example.com", Password = "CorrectHorseBattery1" });

        var result = await service.LoginAsync(new LoginRequestDto { Email = "dm@example.com", Password = "WrongPassword1" });

        Assert.Equal(LoginOutcome.InvalidCredentials, result.Outcome);
        Assert.Null(result.User);
    }

    [Fact]
    public async Task LoginAsync_UnknownEmailAndWrongPassword_ReturnTheSameOutcome()
    {
        var service = CreateService();
        await service.RegisterAsync(new RegisterRequestDto { Email = "dm@example.com", Password = "CorrectHorseBattery1" });

        var unknownEmailResult = await service.LoginAsync(new LoginRequestDto { Email = "nobody@example.com", Password = "CorrectHorseBattery1" });
        var wrongPasswordResult = await service.LoginAsync(new LoginRequestDto { Email = "dm@example.com", Password = "WrongPassword1" });

        // Deliberately indistinguishable, to avoid email enumeration via login.
        Assert.Equal(unknownEmailResult.Outcome, wrongPasswordResult.Outcome);
    }
}
