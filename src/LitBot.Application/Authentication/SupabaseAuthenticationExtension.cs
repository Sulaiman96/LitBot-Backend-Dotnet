using Microsoft.AspNetCore.Authentication;

namespace LitBot.Infrastructure.Authentication;

public static class SupabaseAuthenticationExtension
{
    public static AuthenticationBuilder AddSupabaseAuthentication(
        this AuthenticationBuilder builder,
        Action<SupabaseAuthenticationOptions>? configureOptions = null)
    {
        return builder.AddScheme<SupabaseAuthenticationOptions, SupabaseAuthenticationHandler>(
            "SupabaseAuth", 
            "Supabase Authentication", 
            configureOptions);
    }
}