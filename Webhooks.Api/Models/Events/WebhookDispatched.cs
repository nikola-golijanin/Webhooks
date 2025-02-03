namespace Webhooks.Api.Models.Events;

internal sealed record WebhookDispatched(string EventType, object Data);
