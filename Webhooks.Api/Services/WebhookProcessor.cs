using System.Threading.Channels;

namespace Webhooks.Api.Services;

public sealed class WebhookProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly Channel<WebhookDispatch> _webhooksChannel;

    public WebhookProcessor(
        IServiceScopeFactory serviceScopeFactory,
        Channel<WebhookDispatch> webhooksChannel)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _webhooksChannel = webhooksChannel;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var webhookDispatch in _webhooksChannel.Reader.ReadAllAsync(stoppingToken))
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var dispatcher = scope.ServiceProvider.GetRequiredService<WebhookDispatcher>();
            await dispatcher.ProcessAsync(webhookDispatch.EventType, webhookDispatch.Data);
        }
    }
}
