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
        var httpClient = _httpClientFactory.CreateClient("WebhookClient");

        var payload = new WebhookPayload
        {

            Id = Guid.NewGuid(),
            EventType = context.Message.EventType,
            SubscriptionId = context.Message.SubscriptionId,
            Timestamp = DateTime.UtcNow,
            Data = context.Message.Data
        };

        var deliveryAttempt = new WebhookDeliveryAttempt
        {
            WebhookSubscriptionId = context.Message.SubscriptionId,
            Payload = JsonSerializer.Serialize(payload),
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            var repsonse = await httpClient.PostAsJsonAsync(context.Message.WebhookUrl, payload);
            repsonse.EnsureSuccessStatusCode();

            deliveryAttempt.ReponseStatusCode = (int)repsonse.StatusCode;
            deliveryAttempt.Success = repsonse.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            deliveryAttempt.ReponseStatusCode = null;
            deliveryAttempt.Success = false;
        }

        _dbContext.WebhookDeliveryAttempts.Add(deliveryAttempt);
        await _dbContext.SaveChangesAsync();
    }
}
