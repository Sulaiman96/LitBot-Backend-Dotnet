using LitBot.Core.Interfaces;
using LitBot.Infrastructure.Services;
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
        services.Configure<SupabaseOptions>(
            configuration.GetSection("Supabase"));

        // Register Supabase Client
        services.AddScoped<Client>(provider =>
        {
            var supabaseSection = configuration.GetSection("Supabase");
            var url = supabaseSection["Url"] ?? throw new InvalidOperationException("Supabase URL not configured");
            var key = supabaseSection["Key"] ?? throw new InvalidOperationException("Supabase Key not configured");
            
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