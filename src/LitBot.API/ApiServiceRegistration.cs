using System.Reflection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace LitBot.API;

public static class ApiServiceRegistration
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddHttpContextAccessor();
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = "sb-access-token";
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.LoginPath = "/api/auth/login";
                options.LogoutPath = "/api/auth/logout";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                options.SlidingExpiration = false;

                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/problem+json";

                    var problemDetails = new ProblemDetails
                    {
                        Title = "Unauthorized",
                        Detail = "Authentication required",
                        Status = StatusCodes.Status401Unauthorized,
                        Instance = context.Request.Path
                    };

                    return context.Response.WriteAsJsonAsync(problemDetails);
                };
            });
        
        services.AddAuthorization();
        
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "LitBot API",
                Version = "v1",
                Description = "API for LitBot - AI-powered research paper assistant",
                Contact = new OpenApiContact
                {
                    Name = "LitBot Team",
                    Email = "support@litbotapp.com"
                }
            });

            // Add JWT/Cookie authentication to Swagger
            options.AddSecurityDefinition("cookieAuth", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Cookie,
                Name = "sb-access-token",
                Description = "Authentication cookie"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "cookieAuth"
                        }
                    },
                    []
                }
            });

            // Include XML comments if available
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }
        });
        
        services.AddCors(options =>
        {
            options.AddPolicy("LitBotCors", builder =>
            {
                builder
                    .WithOrigins("http://localhost:3000", "https://localhost:3000", "http://localhost:5079") // Addy => Update this with frontend URL
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials(); // Important for cookies
            });
        });
        
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
        });
        
        services.AddHealthChecks();
        
        return services;
    }
    
    public static WebApplication ConfigureApiPipeline(this WebApplication app)
    {
        // Global exception handler
        app.UseExceptionHandler("/error");
        
        // Add custom exception handling middleware
        app.Use(async (context, next) =>
        {
            try
            {
                await next();
            }
            catch (Exception ex)
            {
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "Unhandled exception occurred");
                
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/problem+json";
                
                var problemDetails = new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred",
                    Status = StatusCodes.Status500InternalServerError,
                    Instance = context.Request.Path
                };
                
                await context.Response.WriteAsJsonAsync(problemDetails);
            }
        });

        // Configure the HTTP request pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "LitBot API v1");
                options.RoutePrefix = "swagger";
            });
        }

        // Security headers
        app.Use(async (context, next) =>
        {
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
            context.Response.Headers.Append("Referrer-Policy", "no-referrer");
            await next();
        });

        // app.UseHttpsRedirection(); //Comment this out if it causes you problems addy
        app.UseResponseCompression();
        
        // Use CORS
        app.UseCors("LitBotCors");

        app.UseAuthentication();
        app.UseAuthorization();
        
        app.MapGet("/", () => Results.Redirect("/swagger"))
            .ExcludeFromDescription();

        app.MapControllers();
        app.MapHealthChecks("/health");
        
        // Map error endpoint
        app.Map("/error", () => Results.Problem());

        return app;
    }
}