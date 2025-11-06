namespace Webhooks.Processing.Models;

public class WebhookSubscription
{
    public long Id { get; set; }
    public string WebhookUrl { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
