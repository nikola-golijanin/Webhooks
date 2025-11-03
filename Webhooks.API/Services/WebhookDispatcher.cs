using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Webhooks.API.Data;
using Webhooks.API.Models;

namespace Webhooks.API.Services;

public sealed class WebhookDispatcher
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly WebhooksDbContext _dbContext;

    public WebhookDispatcher(IHttpClientFactory httpClientFactory, WebhooksDbContext dbContext)
    {
        _httpClientFactory = httpClientFactory;
        _dbContext = dbContext;
    }
    public async Task DispatchAsync<T>(string eventType, T data)
    {
        var subscriptions = await _dbContext.WebhookSubscriptions
            .AsNoTracking()
            .Where(s => s.EventType == eventType)
            .ToListAsync();

        foreach (var subscription in subscriptions)
        {
            using var _httpClient = _httpClientFactory.CreateClient();

            var payload = new WebhookPayload<T>
            {

                Id = Guid.NewGuid(),
                EventType = eventType,
                SubscriptionId = subscription.Id,
                Timestamp = DateTime.UtcNow,
                Data = data
            };
            var jsonPayload = JsonSerializer.Serialize(payload);

            try
            {
                var repsonse = await _httpClient.PostAsJsonAsync(subscription.WebhookUrl, payload);
                var deliveryAttempt = new WebhookDeliveryAttempt
                {
                    WebhookSubscriptionId = subscription.Id,
                    Payload = jsonPayload,
                    ReponseStatusCode = (int)repsonse.StatusCode,
                    Success = repsonse.IsSuccessStatusCode,
                    CreatedAt = DateTime.UtcNow
                };
                _dbContext.WebhookDeliveryAttempts.Add(deliveryAttempt);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception)
            {
                var deliveryAttempt = new WebhookDeliveryAttempt
                {
                    WebhookSubscriptionId = subscription.Id,
                    Payload = jsonPayload,
                    ReponseStatusCode = null,
                    Success = false,
                    CreatedAt = DateTime.UtcNow
                };

                _dbContext.WebhookDeliveryAttempts.Add(deliveryAttempt);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
