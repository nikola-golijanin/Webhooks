using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Webhooks.Infrastructure.Webhooks;
using Webhooks.Persistance;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Host.UseSerilog((context, loggerConfig) =>
            loggerConfig.ReadFrom.Configuration(context.Configuration));
builder.Services.AddOpenApi();
builder.Services.AddHttpClient();
builder.Services.AddDbContext<WebhooksDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));

builder.Services.AddMassTransit(busConfig =>
        {
            busConfig.SetKebabCaseEndpointNameFormatter();

            busConfig.AddConsumer<WebhookDispatchedConsumer>();
            busConfig.AddConsumer<WebhookTriggeredConsumer>();

            busConfig.UsingRabbitMq((context, config) =>
            {
                config.Host(new Uri(builder.Configuration["RabbitMQ:Host"]!), host =>
                {
                    host.Username(builder.Configuration["RabbitMQ:Username"]!);
                    host.Password(builder.Configuration["RabbitMQ:Password"]!);
                });

                config.ConfigureEndpoints(context);
            });
        });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();

