namespace Webhooks.API.Models;

public sealed record WebhookSubscription(long Id, string WebhookUrl, string EventType, DateTime CreatedOnUtc);

public sealed record CreateWebhookRequest(string WebhookUrl, string EventType);
