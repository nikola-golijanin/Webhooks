namespace Webhooks.API.Services;

public class WebhookPayload<T>
{
    public Guid Id { get; set; }
    public string EventType { get; set; }
    public long SubscriptionId { get; set; }
    public DateTime Timestamp { get; set; }
    public T Data { get; set; }
}