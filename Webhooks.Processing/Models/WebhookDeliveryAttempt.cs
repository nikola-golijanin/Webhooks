namespace Webhooks.Processing.Models;

public class WebhookDeliveryAttempt
{
    public long Id { get; set; }
    public long WebhookSubscriptionId { get; set; }
    public string Payload { get; set; }
    public int? ReponseStatusCode { get; set; }
    public bool Success { get; set; }
    public DateTime CreatedAt { get; set; }
}
