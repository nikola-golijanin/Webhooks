namespace Webhooks.Api.Services;

public sealed record WebhookDispatch(string EventType, object Data);
