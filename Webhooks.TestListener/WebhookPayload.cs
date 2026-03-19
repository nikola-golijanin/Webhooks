public record WebhookPayload(
    Guid Id,
    string EventType,
    long SubscriptionId,
    DateTime Timestamp,
    object Data);
