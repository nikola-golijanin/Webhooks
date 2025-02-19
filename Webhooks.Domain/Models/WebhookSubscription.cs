using System.ComponentModel.DataAnnotations.Schema;

namespace Webhooks.Domain.Models;

public sealed class WebhookSubscription
{
    [Column("id")]
    public int Id { get; init; }

    [Column("event_type")]
    public string EventType { get; init; } = string.Empty;

    [Column("webhook_url")]
    public string WebhookUrl { get; init; } = string.Empty;

    [Column("created_on_utc")]
    public DateTime CreatedOnUtc { get; init; }
}