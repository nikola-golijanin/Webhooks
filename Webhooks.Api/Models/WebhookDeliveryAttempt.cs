namespace Webhooks.Api.Models;

public record WebhookDeliveryAttempt(
    Guid Id,
    Guid WebhookSubscriptionId,
    string Payload,
    int? ResponseStatusCode,
    bool Success,
    DateTime Timestamp);
