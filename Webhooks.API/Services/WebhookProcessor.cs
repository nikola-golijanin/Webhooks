using System.Threading.Channels;

namespace Webhooks.API.Services;

public sealed class WebhookProcessor : BackgroundService
{
    private IServiceScopeFactory _serviceScopeFactory;
    private readonly Channel<WebhookDispatch> _channel;

    public WebhookProcessor(Channel<WebhookDispatch> channel, IServiceScopeFactory serviceScopeFactory)
    {
        _channel = channel;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var webhookDispatch in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var dispatcher = scope.ServiceProvider.GetRequiredService<WebhookDispatcher>();
            await dispatcher.ProcessAsync(webhookDispatch.EventType, webhookDispatch.Data);
        }
    }
}
