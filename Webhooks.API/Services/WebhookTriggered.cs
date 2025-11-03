namespace Webhooks.API.Services;

public sealed record WebhookTriggered(long SubscriptionId, string EventType, string WebhookUrl, object Data);
