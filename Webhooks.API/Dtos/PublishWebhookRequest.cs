namespace Webhooks.API.Dtos;

public record PublishWebhookRequest(string EventType, object Payload);