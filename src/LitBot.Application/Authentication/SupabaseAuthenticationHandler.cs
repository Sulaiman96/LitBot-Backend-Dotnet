using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LitBot.Infrastructure.Authentication;

public class SupabaseAuthenticationHandler(
    IOptionsMonitor<SupabaseAuthenticationOptions> options,
    ILoggerFactory logger,
    Supabase.Client supabaseClient,
    UrlEncoder encoder)
    : AuthenticationHandler<SupabaseAuthenticationOptions>(options, logger, encoder)
{
    private readonly ILogger<SupabaseAuthenticationHandler> _logger = logger.CreateLogger<SupabaseAuthenticationHandler>();
    
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        try
        {
            // Check for access token cookie
            if (!Request.Cookies.TryGetValue(Options.AccessTokenCookieName, out var accessToken))
            {
                return AuthenticateResult.NoResult();
            }

            if (string.IsNullOrEmpty(accessToken))
            {
                return AuthenticateResult.NoResult();
            }

            // Validate token with Supabase
            var user = await supabaseClient.Auth.GetUser(accessToken);
            if (user == null)
            {
                return AuthenticateResult.Fail("Invalid access token");
            }

            // Create claims
            var claims = new List<Claim>
            {
                new (ClaimTypes.NameIdentifier, user.Id!),
                new (ClaimTypes.Email, user.Email!),
                new ("access_token", accessToken)
            };

            // Add custom claims if needed
            if (user.UserMetadata.TryGetValue("full_name", out var fullName))
            {
                claims.Add(new Claim(ClaimTypes.Name, fullName.ToString()!));
            }

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authentication failed");
            return AuthenticateResult.Fail($"Authentication failed: {ex.Message}");
        }
    }
    
    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = 401;
        Response.ContentType = "application/problem+json";
        
        var problemDetails = new ProblemDetails
        {
            Title = "Unauthorized",
            Detail = "Authentication required",
            Status = StatusCodes.Status401Unauthorized,
            Instance = Request.Path
        };
        
        return Response.WriteAsJsonAsync(problemDetails);
    }
}