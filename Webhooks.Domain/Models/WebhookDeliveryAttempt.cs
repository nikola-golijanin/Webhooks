using System.ComponentModel.DataAnnotations.Schema;

namespace Webhooks.Domain.Models;

public sealed class WebhookDeliveryAttempt
{
    [Column("id")]
    public int Id { get; init; }

    [Column("webhook_subscription_id")]
    public int WebhookSubscriptionId { get; init; }

    [Column("payload")]
    public string Payload { get; init; } = string.Empty;

    [Column("response_status_code")]
    public int? ResponseStatusCode { get; init; }

    [Column("success")]
    public bool Success { get; init; }

    [Column("timestamp")]
    public DateTime Timestamp { get; init; }
}
