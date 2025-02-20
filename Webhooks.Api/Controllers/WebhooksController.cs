using Microsoft.AspNetCore.Mvc;
using Webhooks.Api.Contracts.Webhooks;
using Webhooks.Domain.Models;
using Webhooks.Infrastructure.Authentication;
using Webhooks.Persistance;
using Permission = Webhooks.Domain.Enums.Permission;

namespace Webhooks.Api.Controllers;

[Route("api/[controller]")]
public class WebhooksController : ApiController
{
    private readonly WebhooksDbContext _context;

    public WebhooksController(WebhooksDbContext context)
    {
        _context = context;
    }

    [HttpPost("subscriptions")]
    [HasPermission(Permission.CreateSubscriptions)]
    public IActionResult CreateSubscription([FromBody] CreateWebhookRequest request)
    {
        var subscription = new WebhookSubscription
        {
            EventType = request.EventType,
            WebhookUrl = request.WebhookUrl,
            CreatedOnUtc = DateTime.UtcNow
        };

        _context.WebhookSubscriptions.Add(subscription);
        _context.SaveChanges();
        return Ok(subscription);
    }
}