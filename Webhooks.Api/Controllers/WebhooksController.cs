using Microsoft.AspNetCore.Mvc;
using Webhooks.Api.Contracts.Webhooks;
using Webhooks.Api.Data;
using Webhooks.Api.Models;

namespace Webhooks.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WebhooksController : ControllerBase
{
    private readonly WebhooksDbContext _context;

    public WebhooksController(WebhooksDbContext context)
    {
        _context = context;
    }

    [HttpPost("subscriptions")]
    public IActionResult CreateSubscription([FromBody] CreateWebhookRequest request)
    {
        var subscription = new WebhookSubscription(
            Guid.NewGuid(),
            request.EventType,
            request.WebhookUrl,
            DateTime.UtcNow);

        _context.WebhookSubscriptions.Add(subscription);
        _context.SaveChanges();
        return Ok(subscription);
    }
}
