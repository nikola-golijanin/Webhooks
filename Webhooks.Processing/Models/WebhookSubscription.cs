namespace Webhooks.Processing.Models;

public class WebhookSubscription
{
    public long Id { get; set; }
    public string WebhookUrl { get; set; }
    public string EventType { get; set; }
    public DateTime CreatedAt { get; set; }
}
