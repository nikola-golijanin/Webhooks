using System;
using Webhooks.Api.Models;
using Webhooks.Api.Repositories;

namespace Webhooks.Api.Services;

public sealed class WebhookDispatcher
{
    private readonly HttpClient _httpClient;
    private readonly InMemoryWebhookSubscriptionRepository _webhookSubscriptionRepository;

    public WebhookDispatcher(
        HttpClient httpClient,
        InMemoryWebhookSubscriptionRepository webhookSubscriptionRepository)
    {
        _webhookSubscriptionRepository = webhookSubscriptionRepository;
        _httpClient = httpClient;
    }

    public async Task DispatchAsync(string eventType, object payload)
    {
        var subscriptions = _webhookSubscriptionRepository.GetByEventType(eventType);

        foreach (WebhookSubscription subscription in subscriptions)
        {
            var request = new
            {
                Id = Guid.NewGuid(),
                subscription.EventType,
                SubscriptionId = subscription.Id,
                Timestamp = DateTime.UtcNow,
                Data = payload
            };

            await _httpClient.PostAsJsonAsync(subscription.WebhookUrl, request);
        }
    }
}
