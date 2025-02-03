
using System.Text.Json;
using MassTransit;
using Webhooks.Api.Data;
using Webhooks.Api.Models;
using Webhooks.Api.Models.Events;

namespace Webhooks.Api.Services.Consumers;

internal sealed class WebhookTriggeredConsumer : IConsumer<WebhookTriggered>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly WebhooksDbContext _context;

    public WebhookTriggeredConsumer(
        IHttpClientFactory httpClientFactory,
        WebhooksDbContext context)
    {
        _httpClientFactory = httpClientFactory;
        _context = context;
    }

    public async Task Consume(ConsumeContext<WebhookTriggered> context)
    {
        using var httpClient = _httpClientFactory.CreateClient();

        var message = context.Message;

        var payload = new WebhookPayload
        {
            Id = Guid.NewGuid(),
            EventType = message.EventType,
            SubscriptionId = message.SubscriptionId,
            Timestamp = DateTime.UtcNow,
            Data = message.Data
        };

        var jsonPayload = JsonSerializer.Serialize(payload);

        try
        {
            var response = await httpClient.PostAsJsonAsync(message.WebhookUrl, jsonPayload);
            response.EnsureSuccessStatusCode();

            var attempt = new WebhookDeliveryAttempt
            (
                Id: Guid.NewGuid(),
                WebhookSubscriptionId: message.SubscriptionId,
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
                WebhookSubscriptionId: message.SubscriptionId,
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
