using LitBot.Core.DTOs.Auth;

namespace LitBot.Core.Interfaces;

public interface IAuthService
{
    Task<UserResponseDto> RegisterUserAsync(UserCreateDto userCreate);
    Task<TokenDto> LoginAsync(UserLoginDto loginDto);
    Task<TokenDto> RefreshTokenAsync(RefreshRequestDto refreshRequest);
    Task<string> ForgotPasswordAsync(ForgotPasswordRequestDto forgotRequest);
    Task<TokenDto> ResetPasswordAsync(ResetPasswordRequestDto resetRequest);
    Task<string> ResendConfirmationAsync(ForgotPasswordRequestDto emailRequest);
    Task<UserResponseDto> GetUserAsync(string token);
}