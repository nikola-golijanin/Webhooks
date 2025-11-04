using MassTransit;
using Microsoft.EntityFrameworkCore;
using Webhooks.Contracts;
using Webhooks.Processing.Data;

namespace Webhooks.Processing.Services;

public sealed class WebhookDispatchedConsumer : IConsumer<WebhookDispatched>
{
    private readonly WebhooksDbContext _dbContext;

    public WebhookDispatchedConsumer(WebhooksDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<WebhookDispatched> context)
    {
        var message = context.Message;

        var subscriptions = await _dbContext.WebhookSubscriptions
            .AsNoTracking()
            .Where(s => s.EventType == message.EventType)
            .ToListAsync();

        foreach (var subscription in subscriptions)
        {
            await context.Publish(new WebhookTriggered(
                SubscriptionId: subscription.Id,
                EventType: message.EventType,
                WebhookUrl: subscription.WebhookUrl,
                Data: message.Data));
        }
    }
}

