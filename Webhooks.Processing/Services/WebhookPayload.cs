namespace Webhooks.Processing.Services;

public class WebhookPayload
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public long SubscriptionId { get; set; }
    public DateTime Timestamp { get; set; }
    public object Data { get; set; } 
}
