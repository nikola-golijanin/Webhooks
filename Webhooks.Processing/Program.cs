using MassTransit;
using Microsoft.EntityFrameworkCore;
using Webhooks.Processing.Data;
using Webhooks.Processing.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddHttpClient();

builder.Services.AddDbContext<WebhooksDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("WebhooksDatabase"));
});

builder.Services.AddMassTransit(busConfig =>
{
    busConfig.SetKebabCaseEndpointNameFormatter();

    busConfig.AddConsumer<WebhookDispatchedConsumer>();
    busConfig.AddConsumer<WebhookTriggeredConsumer>();

    busConfig.UsingRabbitMq((context, config) =>
        {
            config.Host(builder.Configuration["RabbitMQ:Host"]!, host =>
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