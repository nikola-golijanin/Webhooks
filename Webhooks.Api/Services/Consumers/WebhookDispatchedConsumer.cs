using MassTransit;
using Microsoft.EntityFrameworkCore;
using Webhooks.Api.Data;

namespace Webhooks.Api.Services.Consumers;

internal sealed class WebhookDispatchedConsumer : IConsumer<WebhookDispatched>
{
    private readonly WebhooksDbContext _context;

    public WebhookDispatchedConsumer(WebhooksDbContext context)
    {
        _context = context;
    }

    public async Task Consume(ConsumeContext<WebhookDispatched> context)
    {
        var webhookDispatchedEvent = context.Message;

        var subscriptions = await _context.WebhookSubscriptions
                    .AsNoTracking()
                    .Where(ws => ws.EventType == webhookDispatchedEvent.EventType)
                    .ToListAsync();

        foreach (var subscription in subscriptions)
        {
            var payload = new WebhookTriggered
            (
                SubscriptionId: subscription.Id,
                EventType: webhookDispatchedEvent.EventType,
                WebhookUrl: subscription.WebhookUrl,
                Data: webhookDispatchedEvent.Data
            );

            await context.Publish(payload);


            /* Another approach

            await context.PublishBatch(subscriptions.Select(s => new WebhookTriggered
            (
                SubscriptionId: s.Id,
                EventType: webhookDispatchedEvent.EventType,
                WebhookUrl: s.WebhookUrl,
                Payload: webhookDispatchedEvent.Data
            )));

            */
        }
    }
}
