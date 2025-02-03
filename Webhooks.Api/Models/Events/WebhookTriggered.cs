namespace Webhooks.Api.Models.Events;

internal sealed record WebhookTriggered(Guid SubscriptionId, string EventType, string WebhookUrl, object Data);