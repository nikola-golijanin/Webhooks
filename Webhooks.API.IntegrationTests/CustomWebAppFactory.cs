using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Webhooks.API.Data;


namespace Webhooks.API.IntegrationTests;

public class CustomWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithDatabase("webhooks-dev")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType ==
                    typeof(IDbContextOptionsConfiguration<WebhooksDbContext>));

            if (dbContextDescriptor is not null)
            {
                services.Remove(dbContextDescriptor);
            }

            services.AddDbContext<WebhooksDbContext>((container, options) =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
            });

            services.AddMassTransitTestHarness();
        });
    }

    public Task InitializeAsync()
    {
        return _dbContainer.StartAsync();
    }

    Task IAsyncLifetime.DisposeAsync()
    {
        return _dbContainer.StopAsync();
    }
}
