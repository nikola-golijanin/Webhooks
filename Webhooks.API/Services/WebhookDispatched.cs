namespace Webhooks.API.Services;

public sealed record WebhookDispatched(string EventType, object Data);
