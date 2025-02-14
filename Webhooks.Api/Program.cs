using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Serilog;
using Webhooks.Api.Extensions;
using Webhooks.Api.OptionsSetup;
using Webhooks.Application.Abstractions;
using Webhooks.Application.Users;
using Webhooks.Infrastructure.Authentication;

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


builder.Services.AddScoped<IJwtProvider,JwtProvider>();

builder.Services.AddScoped<IUserService,UserService>();

builder.Services.ConfigureOptions<JwtOptionsSetup>();
builder.Services.ConfigureOptions<JwtBearerOptionsSetup>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.AddAuthorization();
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
builder.Services.AddScoped<PermissionService>();

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