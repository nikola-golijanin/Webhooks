using Microsoft.AspNetCore.Mvc;
using Webhooks.Api.Contracts.Webhooks;
using Webhooks.Domain.Enums;
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
    [HasPermission(AuthSchemes: [AuthScheme.Keycloak, AuthScheme.WebhooksApi])]
    [ProducesResponseType(typeof(WebhookSubscription), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Create a new webhook subscription.")]
    [EndpointDescription("Creates a new webhook subscription for the specified event type.")]
    public async Task<IActionResult> CreateSubscriptionAsync([FromBody] CreateWebhookRequest request)
    {
        _logger.LogInformation("Creating webhook subscription for event type {EventType}.", request.EventType);

        var subscription = new WebhookSubscription
        {
            EventType = request.EventType,
            WebhookUrl = request.WebhookUrl,
            CreatedOnUtc = DateTime.UtcNow
        };

        _context.WebhookSubscriptions.Add(subscription);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Webhook subscription created with ID {SubscriptionId}.", subscription.Id);
        return Ok(subscription);
    }
}