namespace Webhooks.Domain.Models;

public sealed record WebhookSubscription(Guid Id, string EventType, string WebhookUrl, DateTime CreatedOnUtc);
