using LitBot.Infrastructure.Services;
using LitBot.Infrastructure.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Supabase;

namespace LitBot.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Register Supabase Client
        services.AddScoped<Client>(_ =>
        {
            var url = configuration["SUPABASE_URL"] ?? throw new InvalidOperationException("Supabase URL not configured");
            var key = configuration["SUPABASE_KEY"] ?? throw new InvalidOperationException("Supabase Key not configured");

            var options = new SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = true
            };

            return new Client(url, key, options);
        });

        // Register services
        services.AddScoped<IAuthService, SupabaseAuthService>();

        return services;
    }
}