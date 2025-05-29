using LitBot.Core.DTOs.Auth;
using LitBot.Core.Interfaces;
using LitBot.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Supabase.Gotrue;
using Client = Supabase.Client;

namespace LitBot.Infrastructure.Services;

public class SupabaseAuthService(
    Client supabaseClient,
    ILogger<SupabaseAuthService> logger,
    IHttpContextAccessor httpContextAccessor) : IAuthService
{

    private const string AccessTokenCookie = "sb-access-token";
    private const string RefreshTokenCookie = "sb-refresh-token";
    
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

    public async Task LoginAsync(UserLoginDto loginDto)
    {
        logger.LogDebug("Attempting to login with email: {Email}", loginDto.Email);

        var signInResponse = await supabaseClient.Auth.SignIn(
            email: loginDto.Email,
            password: loginDto.Password
        );
        
        if(signInResponse?.User == null || signInResponse.AccessToken == null)
            throw new UnauthorizedAccessException("Invalid email or password.");

        SetAuthCookies(signInResponse.AccessToken, signInResponse.RefreshToken!);
    }

    public async Task RefreshTokenAsync()
    {
        logger.LogDebug("Attempting to refresh token");
        
        var context = httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HttpContext not available");
        
        var refreshToken = context.Request.Cookies[RefreshTokenCookie];
        if (string.IsNullOrEmpty(refreshToken))
            throw new UnauthorizedAccessException("No refresh token found.");

        var refreshedSession =  await supabaseClient.Auth.RefreshSession();

        if (refreshedSession?.User == null || refreshedSession.AccessToken == null)
        {
            ClearAuthCookies();
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }
        
        SetAuthCookies(refreshedSession.AccessToken, refreshedSession.RefreshToken!);
    }

    public async Task<string> ForgotPasswordAsync(ForgotPasswordRequestDto forgotRequest)
    {
        logger.LogDebug("Password reset request for email: {Email}", forgotRequest.Email);
        
        await supabaseClient.Auth.ResetPasswordForEmail(forgotRequest.Email);

        return "Password reset email sent successfully";
    }

    public async Task<string> ResetPasswordAsync(ResetPasswordRequestDto resetRequest)
    {
        logger.LogDebug("Attempting to reset password with token");

        try
        {
            var session = await supabaseClient.Auth.VerifyOTP(
                email: null!,
                token: resetRequest.Token,
                type: Constants.EmailOtpType.Recovery
            );

            if (session?.User == null)
                throw new UnauthorizedAccessException("Invalid or expired reset token.");

            var updateResponse = await supabaseClient.Auth.Update(new UserAttributes
            {
                Password = resetRequest.NewPassword
            });

            if (updateResponse == null)
                throw new Exception("Failed to update password.");

            return "Password reset successfully";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to reset password.");
            throw new UnauthorizedAccessException("Invalid or expired reset token");
        }
    }
    
    public async Task<UserResponseDto> GetUserAsync()
    {
        logger.LogDebug("Attempting to get current user from token");
        
        var context =  httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HttpContext not available");
        
        var accessToken = context.Request.Cookies[AccessTokenCookie];
        if(string.IsNullOrEmpty(accessToken))
            throw new UnauthorizedAccessException("No access token found.");
        
        var user = await supabaseClient.Auth.GetUser(accessToken);
        if(user == null)
            throw new UnauthorizedAccessException("Invalid access token.");
        
        var profile = await GetUserProfile(Guid.Parse(user.Id!));

        return new UserResponseDto
        {
            Id = user.Id!,
            Email = user.Email!,
            FirstName = profile?.FirstName ?? string.Empty,
            LastName = profile?.LastName ?? string.Empty,
            ConfirmedAt = user.ConfirmedAt,
        };
    }
    
    private void SetAuthCookies(string accessToken, string refreshToken)
    {
        var context = httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HttpContext not available");
        
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = false, // TODO - Set to true in production with HTTPS
            SameSite = SameSiteMode.Strict,
            Path = "/"
        };

        // Access token - shorter expiry
        var accessCookieOptions = cookieOptions;
        accessCookieOptions.Expires = DateTime.UtcNow.AddHours(1);
        context.Response.Cookies.Append(AccessTokenCookie, accessToken, accessCookieOptions);

        // Refresh token - longer expiry
        var refreshCookieOptions = cookieOptions;
        refreshCookieOptions.Expires = DateTime.UtcNow.AddDays(7);
        context.Response.Cookies.Append(RefreshTokenCookie, refreshToken, refreshCookieOptions);

        logger.LogDebug("Auth cookies set successfully");
    }
    
    private void ClearAuthCookies()
    {
        var context = httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HttpContext not available");
        
        context.Response.Cookies.Delete(AccessTokenCookie);
        context.Response.Cookies.Delete(RefreshTokenCookie);
        logger.LogDebug("Auth cookies cleared");
    }
    
    private async Task<Profile?> GetUserProfile(Guid userId)
    {
        try
        {
            var response = await supabaseClient
                .From<Profile>()
                .Where(p => p.UserId == userId)
                .Single();
            
            return response;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not retrieve user profile for user {UserId}", userId);
            return null;
        }
    }
}