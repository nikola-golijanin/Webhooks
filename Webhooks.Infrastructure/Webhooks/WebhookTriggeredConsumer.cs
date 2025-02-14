using System.Net.Http.Json;
using System.Text.Json;
using MassTransit;
using Microsoft.Extensions.Logging;
using Webhooks.Domain.Events;
using Webhooks.Domain.Models;
using Webhooks.Persistance;

namespace Webhooks.Infrastructure.Webhooks;

public sealed class WebhookTriggeredConsumer : IConsumer<WebhookTriggered>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly WebhooksDbContext _context;
    private readonly ILogger<WebhookTriggeredConsumer> _logger;

    public WebhookTriggeredConsumer(
        IHttpClientFactory httpClientFactory,
        WebhooksDbContext context,
        ILogger<WebhookTriggeredConsumer> logger)
    {
        _httpClientFactory = httpClientFactory;
        _context = context;
        _logger = logger;
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

            _logger.LogInformation("Webhook delivery to {WebhookUrl} successful. {SubscriptionId}, {StatusCode}",
                message.WebhookUrl,
                message.SubscriptionId,
                response.StatusCode);

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
        catch (Exception ex)
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

            _logger.LogError(ex, "Webhook delivery to {WebhookUrl} failed. {SubscriptionId}",
                message.WebhookUrl,
                message.SubscriptionId);

            _context.WebhookDeliveryAttempts.Add(attempt);
            await _context.SaveChangesAsync();
        }

    }
}
