using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Webhooks.Api.Data;
using Webhooks.Api.Models;

namespace Webhooks.Api.Services;

public sealed class WebhookDispatcher
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly WebhooksDbContext _context;

    public WebhookDispatcher(
        IHttpClientFactory httpClientFactory,
        WebhooksDbContext context)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
    }

    public async Task DispatchAsync<T>(string eventType, T data)
    {
        var subscriptions = await _context.WebhookSubscriptions
            .AsNoTracking()
            .Where(ws => ws.EventType == eventType)
            .ToListAsync();

        using var httpClient = _httpClientFactory.CreateClient();

        foreach (var subscription in subscriptions)
        {
            var payload = new WebhookPayload<T>
            {
                Id = Guid.NewGuid(),
                EventType = subscription.EventType,
                SubscriptionId = subscription.Id,
                Timestamp = DateTime.UtcNow,
                Data = data
            };

            var jsonPayload = JsonSerializer.Serialize(payload);

            try
            {
                var response = await httpClient.PostAsJsonAsync(subscription.WebhookUrl, jsonPayload);
                var attempt = new WebhookDeliveryAttempt
                {
                    Id = Guid.NewGuid(),
                    WebhookSubscriptionId = subscription.Id,
                    Payload = jsonPayload,
                    ReponseStatusCode = (int)response.StatusCode,
                    Success = response.IsSuccessStatusCode,
                    Timestamp = DateTime.UtcNow
                };

                _context.WebhookDeliveryAttempts.Add(attempt);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                var attempt = new WebhookDeliveryAttempt
                {
                    Id = Guid.NewGuid(),
                    WebhookSubscriptionId = subscription.Id,
                    Payload = jsonPayload,
                    ReponseStatusCode = null,
                    Success = false,
                    Timestamp = DateTime.UtcNow
                };

                _context.WebhookDeliveryAttempts.Add(attempt);
                await _context.SaveChangesAsync();
            }
        }
    }
}
