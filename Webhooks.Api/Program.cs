using Microsoft.AspNetCore.Authentication.JwtBearer;
using Serilog;
using Webhooks.Api.Authentication;
using Webhooks.Api.Authentication.OptionsSetup;
using Webhooks.Api.Extensions;
using Webhooks.Api.Services.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddSerilog();

builder.Services.AddOpenApi();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddHttpClient();

builder.Services.AddServices();

builder.Services.AddDatabaseContext(builder.Configuration);

builder.Services.AddMassTransitServices(builder.Configuration);

builder.Services.AddOpenTelemetryTracingAndMetrics();

builder.Services.AddScoped<UserService>();

builder.Services.AddScoped<JwtProvider>();

builder.Services.ConfigureOptions<JwtOptionsSetup>();
builder.Services.ConfigureOptions<JwtBearerOptionsSetup>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsDocker())
{
    app.MapOpenApi();
    app.AddScalarUi();
    await app.ApplyMigrationAsync();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.UseSerilogPlusRequestLogging();

app.MapControllers();

app.Run();