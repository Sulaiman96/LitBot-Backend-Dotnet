using LitBot.Core.DTOs.Auth;
using LitBot.Core.Interfaces;
using LitBot.Infrastructure.Models;
using Microsoft.Extensions.Logging;
using Supabase;

namespace LitBot.Infrastructure.Services;

public class SupabaseAuthService(Client supabaseClient, ILogger<SupabaseAuthService> logger) : IAuthService
{

    public async Task<UserResponseDto> RegisterUserAsync(UserCreateDto userCreateDto)
    {
        logger.LogDebug("Supabase sign up -> {Email}", userCreateDto.Email);

        var signUpResponse = await supabaseClient.Auth.SignUp(
            email: userCreateDto.Email,
            password: userCreateDto.Password
        );

        if (signUpResponse?.User == null)
            throw new Exception("Failed to register user. Email may already be in use.");

        var profile = new Profile
        {
            UserId = Guid.Parse(signUpResponse.User.Id!),
            FirstName = userCreateDto.FirstName,
            LastName = userCreateDto.LastName,
            CreatedAt = DateTime.UtcNow
        };
        
        await supabaseClient.From<Profile>().Insert(profile);
        
        return new UserResponseDto
        {
            Id = signUpResponse.User?.Id!,
            Email = userCreateDto.Email,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            ConfirmedAt = signUpResponse.User?.ConfirmedAt ?? null
        };
    }

    public Task<TokenDto> LoginAsync(UserLoginDto loginDto)
    {
        logger.LogDebug("Attempting to login with following email: {Email}", loginDto.Email);
        throw new NotImplementedException();
    }

    public Task<TokenDto> RefreshTokenAsync(RefreshRequestDto refreshRequest)
    {
        throw new NotImplementedException();
    }

    public Task<string> ForgotPasswordAsync(ForgotPasswordRequestDto forgotRequest)
    {
        throw new NotImplementedException();
    }

    public Task<TokenDto> ResetPasswordAsync(ResetPasswordRequestDto resetRequest)
    {
        throw new NotImplementedException();
    }

    public Task<string> ResendConfirmationAsync(ForgotPasswordRequestDto emailRequest)
    {
        throw new NotImplementedException();
    }

    public Task<UserResponseDto> GetUserAsync(string token)
    {
        throw new NotImplementedException();
    }
}