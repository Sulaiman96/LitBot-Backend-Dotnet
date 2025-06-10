using LitBot.API;
using LitBot.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.WriteTo.Console());
        
// Add services from API layer
builder.Services.AddApiServices();

// Add services from Infrastructure layer
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline from API layer
app.ConfigureApiPipeline();

app.Run();