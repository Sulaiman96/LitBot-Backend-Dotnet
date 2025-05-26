using LitBot.Core.DTOs.Auth;
using LitBot.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Supabase;

namespace LitBot.Infrastructure.Services;

public class SupabaseAuthService(Client supabaseClient, ILogger<SupabaseAuthService> logger) : IAuthService
{

    public async Task<UserResponseDto> RegisterUserAsync(UserCreateDto userCreate)
    {
        try
        {
            logger.LogDebug("Supabase sign up -> {Email}", userCreate.Email);

            var response = await supabaseClient.Auth.SignUp(userCreate.Email, userCreate.Password);

            if (response?.User == null)
            {
                throw new InvalidOperationException("Registration failed - no user returned");
            }

            // Check if profile already exists
            var existingProfile = await supabaseClient
                .From<LitBot.Core.Models.Profile>()
                .Where(p => p.UserId == response.User.Id)
                .Get();

            if (existingProfile.Models.Any())
            {
                throw new InvalidOperationException("User already exists with this email address");
            }

            // Create profile
            var profile = new LitBot.Core.Models.Profile
            {
                UserId = response.User.Id,
                FirstName = userCreate.FirstName,
                LastName = userCreate.LastName,
                ProfilePic = null,
                CurrentDailyToken = 50000
            };

            var profileResponse = await supabaseClient
                .From<LitBot.Core.Models.Profile>()
                .Insert(profile);

            if (!profileResponse.Models.Any())
            {
                logger.LogError("Failed to create profile");
                // Attempt rollback
                await supabaseClient.Auth.Admin.DeleteUser(response.User.Id);
                throw new InvalidOperationException("Failed to create profile; registration rolled back");
            }

            logger.LogDebug("Profile inserted successfully");

            return new UserResponseDto
            {
                Id = response.User.Id,
                Email = response.User.Email ?? userCreate.Email,
                FirstName = userCreate.FirstName,
                LastName = userCreate.LastName,
                ConfirmedAt = response.User.ConfirmedAt
            };
        }
        catch (Supabase.Gotrue.Exceptions.GotrueException ex)
        {
            logger.LogError(ex, "Supabase auth error during registration");
            throw new UnauthorizedAccessException(ex.Message);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected registration failure");
            throw new InvalidOperationException("Registration failed");
        }
    }

    public async Task<TokenDto> LoginAsync(UserLoginDto loginDto)
    {
        try
        {
            logger.LogInformation("Attempting login for user: {Email}", loginDto.Email);

            var response = await supabaseClient.Auth.SignIn(loginDto.Email, loginDto.Password);

            if (response?.Session == null)
            {
                logger.LogError("Login failed: No session data returned");
                throw new UnauthorizedAccessException("Invalid login credentials");
            }

            var session = response.Session;
            logger.LogInformation("Successfully logged in user: {Email}", loginDto.Email);

            return new TokenDto
            {
                AccessToken = session.AccessToken,
                RefreshToken = session.RefreshToken,
                ExpiresIn = session.ExpiresIn,
                TokenType = "bearer"
            };
        }
        catch (Supabase.Gotrue.Exceptions.GotrueException ex)
        {
            logger.LogError(ex, "Supabase auth error during login");
            throw new UnauthorizedAccessException(ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected login failure");
            throw new InvalidOperationException("Login failed");
        }
    }

    public async Task<TokenDto> RefreshTokenAsync(RefreshRequestDto refreshRequest)
    {
        try
        {
            logger.LogInformation("Attempting to refresh token");

            var response = await supabaseClient.Auth.RefreshSession(refreshRequest.RefreshToken);

            if (response?.Session == null)
            {
                logger.LogError("Token refresh failed: No session data returned");
                throw new UnauthorizedAccessException("Invalid or expired refresh token");
            }

            var session = response.Session;
            logger.LogInformation("Successfully refreshed token");

            return new TokenDto
            {
                AccessToken = session.AccessToken,
                RefreshToken = session.RefreshToken,
                ExpiresIn = session.ExpiresIn,
                TokenType = "bearer"
            };
        }
        catch (Supabase.Gotrue.Exceptions.GotrueException ex)
        {
            logger.LogError(ex, "Supabase auth error during token refresh");
            throw new UnauthorizedAccessException(ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected refresh failure");
            throw new InvalidOperationException("Refresh failed");
        }
    }

    public async Task<string> ForgotPasswordAsync(ForgotPasswordRequestDto forgotRequest)
    {
        try
        {
            await supabaseClient.Auth.ResetPasswordForEmail(forgotRequest.Email, _redirectUrl);
            return "Password-reset email sent";
        }
        catch (Supabase.Gotrue.Exceptions.GotrueException ex)
        {
            logger.LogError(ex, "Supabase auth error during forgot password");
            throw new UnauthorizedAccessException(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected forgot password failure");
            throw new InvalidOperationException("Forgot password failed");
        }
    }

    public async Task<TokenDto> ResetPasswordAsync(ResetPasswordRequestDto resetRequest)
    {
        try
        {
            var userAttributes = new Dictionary<string, object>
            {
                { "password", resetRequest.NewPassword }
            };
            
            var response = await supabaseClient.Auth.UpdateUser(userAttributes, resetRequest.Token);

            if (response?.Session == null)
            {
                throw new InvalidOperationException("Invalid or expired reset token");
            }

            var session = response.Session;

            return new TokenDto
            {
                AccessToken = session.AccessToken,
                RefreshToken = session.RefreshToken,
                ExpiresIn = session.ExpiresIn,
                TokenType = "bearer"
            };
        }
        catch (Supabase.Gotrue.Exceptions.GotrueException ex)
        {
            logger.LogError(ex, "Supabase auth error during password reset");
            throw new UnauthorizedAccessException(ex.Message);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected reset token failure");
            throw new InvalidOperationException("Reset token failed");
        }
    }

    public async Task<string> ResendConfirmationAsync(ForgotPasswordRequestDto emailRequest)
    {
        try
        {
            await supabaseClient.Auth.Resend(Supabase.Gotrue.Constants.EmailType.Signup, emailRequest.Email, _redirectUrl);
            return "Confirmation email sent";
        }
        catch (Supabase.Gotrue.Exceptions.GotrueException ex)
        {
            logger.LogError(ex, "Supabase auth error during confirmation resend");
            throw new UnauthorizedAccessException(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected resend email failure");
            throw new InvalidOperationException("Resend email failed");
        }
    }

    public async Task<UserResponseDto> GetUserAsync(string token)
    {
        try
        {
            var response = await supabaseClient.Auth.GetUser(token);

            if (response?.User == null)
            {
                throw new UnauthorizedAccessException("Invalid token or user not found");
            }

            var user = response.User;

            // Get profile data
            var profileResponse = await supabaseClient
                .From<LitBot.Core.Models.Profile>()
                .Where(p => p.UserId == user.Id)
                .Get();

            var profile = profileResponse.Models.FirstOrDefault();

            return new UserResponseDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = profile?.FirstName ?? string.Empty,
                LastName = profile?.LastName ?? string.Empty,
                ConfirmedAt = user.ConfirmedAt
            };
        }
        catch (Supabase.Gotrue.Exceptions.GotrueException ex)
        {
            logger.LogError(ex, "Supabase auth error getting user");
            throw new UnauthorizedAccessException(ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected get user failure");
            throw new InvalidOperationException("Get user failed");
        }
    }
}