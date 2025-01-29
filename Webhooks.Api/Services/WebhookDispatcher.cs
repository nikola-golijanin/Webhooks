using System.Text.Json;
using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using Webhooks.Api.Data;
using Webhooks.Api.Models;

namespace Webhooks.Api.Services;
public sealed class WebhookDispatcher
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly WebhooksDbContext _context;
    private readonly Channel<WebhookDispatch> _webhooksChannel;

    public WebhookDispatcher(
        IHttpClientFactory httpClientFactory,
        WebhooksDbContext context,
        Channel<WebhookDispatch> webhooksChannel)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _webhooksChannel = webhooksChannel;
    }
    public async Task DispatchAsync<T>(string eventType, T data) where T : notnull
    {
        await _webhooksChannel.Writer.WriteAsync(new WebhookDispatch(eventType, data));
    }

    public async Task ProcessAsync<T>(string eventType, T data)
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
                (
                    Id: Guid.NewGuid(),
                    WebhookSubscriptionId: subscription.Id,
                    Payload: jsonPayload,
                    ResponseStatusCode: (int)response.StatusCode,
                    Success: response.IsSuccessStatusCode,
                    Timestamp: DateTime.UtcNow
                );

                _context.WebhookDeliveryAttempts.Add(attempt);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                var attempt = new WebhookDeliveryAttempt
                (
                    Id: Guid.NewGuid(),
                    WebhookSubscriptionId: subscription.Id,
                    Payload: jsonPayload,
                    ResponseStatusCode: null,
                    Success: false,
                    Timestamp: DateTime.UtcNow

                );

                _context.WebhookDeliveryAttempts.Add(attempt);
                await _context.SaveChangesAsync();
            }
        }
    }
}
