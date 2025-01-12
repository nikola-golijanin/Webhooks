namespace Webhooks.Api.Contracts.Webhooks;

public sealed record CreateWebhookRequest(string EventType, string WebhookUrl);
