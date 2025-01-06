using Webhooks.Api.Models;

namespace Webhooks.Api.Repositories;

public sealed class InMemoryWebhookSubscriptionRepository
{
    private readonly List<WebhookSubscription> _subscriptions = [];

    public void Add(WebhookSubscription order)
    {
        _subscriptions.Add(order);
    }

    public IReadOnlyList<WebhookSubscription> GetByEventType(string eventType)
    {
        return _subscriptions.Where(s => s.EventType == eventType)
                             .ToList()
                             .AsReadOnly();
    }
}
