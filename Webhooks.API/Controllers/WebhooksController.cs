using Microsoft.AspNetCore.Mvc;
using Webhooks.API.Models;
using Webhooks.API.Repositories;

namespace Webhooks.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WebhooksController : ControllerBase
{
    private readonly InMemoryWebhookSubscriptionRepository _subscriptionRepository;

    public WebhooksController(InMemoryWebhookSubscriptionRepository subscriptionRepository)
    {
        _subscriptionRepository = subscriptionRepository;
    }

    [HttpPost("subscribtions")]
    public IActionResult CreateSubscription([FromBody] CreateWebhookRequest request)
    {
        WebhookSubscription newSubscription = new WebhookSubscription(
            Id: new Random().Next(1, 1000),
            WebhookUrl: request.WebhookUrl,
            EventType: request.EventType,
            CreatedOnUtc: DateTime.UtcNow
        );

        _subscriptionRepository.Add(newSubscription);

        return Ok(newSubscription);
    }
}
