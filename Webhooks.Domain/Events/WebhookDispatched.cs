namespace Webhooks.Domain.Events;

public sealed record WebhookDispatched(string EventType, object Data);