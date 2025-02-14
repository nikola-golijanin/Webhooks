namespace Webhooks.Domain.Events;

public sealed record WebhookTriggered(Guid SubscriptionId, string EventType, string WebhookUrl, object Data);