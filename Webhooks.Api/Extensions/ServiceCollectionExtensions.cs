using MassTransit;
using Microsoft.EntityFrameworkCore;
using Webhooks.Api.Data;
using Webhooks.Api.Services.Consumers;
using Webhooks.Api.Services.Publishers;

namespace Webhooks.Api.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Registers application services into the dependency injection container.
    /// </summary>
    /// <param name="services">The IServiceCollection instance.</param>
    public static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<WebhookDispatcher>();
    }

    /// <summary>
    ///     Configures and registers the database context using PostgreSQL.
    ///     Reads the connection string from the provided configuration.
    /// </summary>
    /// <param name="services">The IServiceCollection instance.</param>
    /// <param name="configuration">The application configuration to retrieve connection string.</param>
    public static void AddDatabaseContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<WebhooksDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("PostgresConnection")));
    }

    /// <summary>
    ///     Configures MassTransit with RabbitMQ for event-driven messaging.
    ///     Registers consumers and sets up the message broker connection.
    /// </summary>
    /// <param name="services">The IServiceCollection instance.</param>
    /// <param name="configuration">The application configuration for RabbitMQ settings.</param>
    public static void AddMassTransitServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(busConfig =>
        {
            busConfig.SetKebabCaseEndpointNameFormatter();

            busConfig.AddConsumer<WebhookDispatchedConsumer>();
            busConfig.AddConsumer<WebhookTriggeredConsumer>();

            busConfig.UsingRabbitMq((context, config) =>
            {
                config.Host(new Uri(configuration["RabbitMQ:Host"]!), host =>
                {
                    host.Username(configuration["RabbitMQ:Username"]!);
                    host.Password(configuration["RabbitMQ:Password"]!);
                });

                config.ConfigureEndpoints(context);
            });
        });
    }
}