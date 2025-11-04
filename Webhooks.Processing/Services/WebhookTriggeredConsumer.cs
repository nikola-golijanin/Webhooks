using System;
using System.Text.Json;
using MassTransit;
using Webhooks.Contracts;
using Webhooks.Processing.Data;
using Webhooks.Processing.Models;

namespace Webhooks.Processing.Services;

public sealed class WebhookTriggeredConsumer : IConsumer<WebhookTriggered>
{
    private readonly WebhooksDbContext _dbContext;
    private readonly IHttpClientFactory _httpClientFactory;

    public WebhookTriggeredConsumer(WebhooksDbContext dbContext, IHttpClientFactory httpClientFactory)
    {
        _dbContext = dbContext;
        _httpClientFactory = httpClientFactory;
    }

    public async Task Consume(ConsumeContext<WebhookTriggered> context)
    {
        using var _httpClient = _httpClientFactory.CreateClient();

        var payload = new WebhookPayload
        {

            Id = Guid.NewGuid(),
            EventType = context.Message.EventType,
            SubscriptionId = context.Message.SubscriptionId,
            Timestamp = DateTime.UtcNow,
            Data = context.Message.Data
        };
        var jsonPayload = JsonSerializer.Serialize(payload);

        try
        {
            var repsonse = await _httpClient.PostAsJsonAsync(context.Message.WebhookUrl, payload);
            repsonse.EnsureSuccessStatusCode();

            var deliveryAttempt = new WebhookDeliveryAttempt
            {
                WebhookSubscriptionId = context.Message.SubscriptionId,
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
                WebhookSubscriptionId = context.Message.SubscriptionId,
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
