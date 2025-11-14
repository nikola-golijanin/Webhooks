namespace Webhooks.API.Dtos;

public sealed record CreateWebhookRequest(string WebhookUrl, string EventType);
