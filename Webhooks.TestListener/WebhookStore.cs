using System.Collections.Concurrent;

public sealed class WebhookStore
{
    private readonly ConcurrentQueue<ReceivedWebhook> _webhooks = new();

    public void Add(WebhookPayload payload) =>
        _webhooks.Enqueue(new ReceivedWebhook(payload, DateTime.UtcNow));

    public IReadOnlyList<ReceivedWebhook> GetAll() => _webhooks.ToArray();

    public int Count => _webhooks.Count;

    public void Clear() => _webhooks.Clear();
}

public record ReceivedWebhook(WebhookPayload Payload, DateTime ReceivedAt);
