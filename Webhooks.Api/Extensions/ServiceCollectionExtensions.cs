using MassTransit;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Webhooks.Api.Data;
using Webhooks.Api.Services.Consumers;
using Webhooks.Api.Services.Publishers;

namespace Webhooks.Api.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Configures and registers Serilog as the logging provider for the application.
    ///     Reads logging configuration from the application's configuration settings.
    /// </summary>
    /// <param name="builder">The WebApplicationBuilder instance.</param>
    public static void AddSerilog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, loggerConfig) =>
            loggerConfig.ReadFrom.Configuration(context.Configuration));
    }


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

    /// <summary>
    /// Configures OpenTelemetry tracing and metrics for the application.
    /// </summary>
    /// <param name="services">The IServiceCollection to add the OpenTelemetry services to.</param>
    /// <remarks>
    /// This method sets up OpenTelemetry for tracing and metrics with the following configurations:
    /// <list type="bullet">
    /// <item>
    /// <description>Adds a resource with the service name "Webhooks.Api".</description>
    /// </item>
    /// <item>
    /// <description>Configures tracing with ASP.NET Core and HTTP client instrumentation, and adds an OTLP exporter.</description>
    /// </item>
    /// <item>
    /// <description>Configures metrics with ASP.NET Core and HTTP client instrumentation, and adds an OTLP exporter.</description>
    /// </item>
    /// </list>
    /// </remarks>
    public static void AddOpenTelemetryTracingAndMetrics(this IServiceCollection services)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService("Webhooks.Api"))
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation()
                       .AddHttpClientInstrumentation();

                tracing.AddOtlpExporter();
            })
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation();

                metrics.AddOtlpExporter();
            });
    }
}