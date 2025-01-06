using Microsoft.AspNetCore.Mvc;
using Webhooks.Api.Models;
using Webhooks.Api.Repositories;

namespace Webhooks.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WebhooksController : ControllerBase
{

    private readonly InMemoryWebhookSubscriptionRepository _webhookSubscriptionRepository;

    public WebhooksController(InMemoryWebhookSubscriptionRepository webhookSubscriptionRepository)
    {
        _webhookSubscriptionRepository = webhookSubscriptionRepository;
    }

    [HttpPost("subscriptions")]
    public IActionResult CreateSubscription([FromBody] CreateWebhookRequest request)
    {
        var subscription = new WebhookSubscription(
            Guid.NewGuid(),
            request.EventType,
            request.WebhookUrl,
            DateTime.UtcNow);

        _webhookSubscriptionRepository.Add(subscription);
        return Ok(subscription);
    }
}
