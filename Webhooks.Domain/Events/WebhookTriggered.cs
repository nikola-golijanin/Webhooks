namespace Webhooks.Domain.Events;

public sealed record WebhookTriggered(int SubscriptionId, string EventType, string WebhookUrl, object Data);