namespace Webhooks.Api.Models;

public record WebhookDeliveryAttempt(
    Guid Id,
    Guid WebhookSubscriptionId,
    string Payload,
    int? ReponseStatusCode,
    bool Success,
    DateTime Timestamp);
