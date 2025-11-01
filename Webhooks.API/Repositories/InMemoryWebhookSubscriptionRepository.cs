namespace Webhooks.API.Repositories;

using Models;

public sealed class InMemoryWebhookSubscriptionRepository
{
    private readonly List<WebhookSubscription> _subscriptions = [];

    public void Add(WebhookSubscription subscription)
    {
        _subscriptions.Add(subscription);
    }

    public IReadOnlyList<WebhookSubscription> GetByEventType(string eventType)
    {
        return _subscriptions
            .Where(s => s.EventType.Equals(eventType, StringComparison.OrdinalIgnoreCase))
            .ToList()
            .AsReadOnly();
    }
}