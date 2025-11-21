using Microsoft.Extensions.DependencyInjection;
using Webhooks.API.Data;
using Webhooks.API.Repositories;

namespace Webhooks.API.IntegrationTests;

public abstract class BaseIntegrationTest : IClassFixture<CustomWebAppFactory>
{
    private readonly IServiceScope _scope;

    protected WebhooksDbContext DbContext;

    protected InMemoryOrderRepository InMemoryOrderRepository;
    

    public BaseIntegrationTest(CustomWebAppFactory factory)
    {
        _scope = factory.Services.CreateScope();
        DbContext = _scope.ServiceProvider.GetRequiredService<WebhooksDbContext>();
        InMemoryOrderRepository = _scope.ServiceProvider.GetRequiredService<InMemoryOrderRepository>();
    }
}
