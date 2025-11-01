using Webhooks.API.Repositories;

namespace Webhooks.API.Services;

public sealed class WebhookDispatcher
{
    private readonly HttpClient _httpClient;
    private readonly InMemoryWebhookSubscriptionRepository _subscriptionRepository;

    public WebhookDispatcher(InMemoryWebhookSubscriptionRepository subscriptionRepository, HttpClient httpClient)
    {
        _subscriptionRepository = subscriptionRepository;
        _httpClient = httpClient;
    }
    public async Task DispatchAsync(string eventType, object eventData)
    {
        var subscriptions = _subscriptionRepository.GetByEventType(eventType);

        foreach (var subscription in subscriptions)
        {
            var request = new
            {
                Id = Guid.NewGuid(),
                subscription.EventType,
                SubscriptionId = subscription.Id,
                Timestamp = DateTime.UtcNow,
                Payload = eventData
            };
            await _httpClient.PostAsJsonAsync(subscription.WebhookUrl, request);
        }
    }
}
