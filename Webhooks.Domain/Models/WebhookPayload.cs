using System.ComponentModel.DataAnnotations.Schema;

namespace Webhooks.Domain.Models;

public sealed record WebhookPayload
{
    public int Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public int SubscriptionId { get; set; }
    public DateTime Timestamp { get; set; }
    public object Data { get; set; } = default!;
}