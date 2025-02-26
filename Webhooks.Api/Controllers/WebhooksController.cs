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
    private readonly ILogger<WebhooksController> _logger;

    public WebhooksController(WebhooksDbContext context, ILogger<WebhooksController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost("subscriptions")]
    [HasPermission(Permission.CreateSubscriptions)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult CreateSubscription([FromBody] CreateWebhookRequest request)
    {
        _logger.LogInformation("Creating webhook subscription for event type {EventType}.", request.EventType);

        var subscription = new WebhookSubscription
        {
            EventType = request.EventType,
            WebhookUrl = request.WebhookUrl,
            CreatedOnUtc = DateTime.UtcNow
        };

        _context.WebhookSubscriptions.Add(subscription);
        _context.SaveChanges();
        _logger.LogInformation("Webhook subscription created with ID {SubscriptionId}.", subscription.Id);
        return Ok(subscription);
    }
}