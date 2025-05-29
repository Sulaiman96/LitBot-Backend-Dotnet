using LitBot.Core.DTOs.Auth;

namespace LitBot.Core.Interfaces;

public interface IAuthService
{
    Task<UserResponseDto> RegisterUserAsync(UserCreateDto userCreate);
    Task LoginAsync(UserLoginDto loginDto);
    Task RefreshTokenAsync();
    Task<string> ForgotPasswordAsync(ForgotPasswordRequestDto forgotRequest);
    Task<string> ResetPasswordAsync(ResetPasswordRequestDto resetRequest);
    Task<UserResponseDto> GetUserAsync();
}