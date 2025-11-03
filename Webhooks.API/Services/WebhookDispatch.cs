namespace Webhooks.API.Services;

public sealed record WebhookDispatch(string EventType, object Data);
