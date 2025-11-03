using MassTransit;

namespace Webhooks.API.Services;

public sealed class WebhookDispatcher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public WebhookDispatcher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task DispatchAsync<T>(string eventType, T data)
        where T : notnull
    {
        await _publishEndpoint.Publish(new WebhookDispatched(eventType, data));
    }
}
