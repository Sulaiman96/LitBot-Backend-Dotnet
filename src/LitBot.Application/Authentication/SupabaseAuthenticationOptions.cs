using Microsoft.AspNetCore.Authentication;

namespace LitBot.Infrastructure.Authentication;

public class SupabaseAuthenticationOptions : AuthenticationSchemeOptions
{
    public string AccessTokenCookieName { get; set; } = "sb-access-token";
    public string RefreshTokenCookieName { get; set; } = "sb-refresh-token";
}