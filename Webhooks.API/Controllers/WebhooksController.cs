using Microsoft.AspNetCore.Mvc;
using Webhooks.API.Data;
using Webhooks.API.Models;
using Webhooks.API.Services;

namespace Webhooks.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WebhooksController : ControllerBase
{
    private readonly WebhooksDbContext _dbContext;
    private readonly WebhookDispatcher _webhookDispatcher;

    public WebhooksController(WebhooksDbContext dbContext, WebhookDispatcher webhookDispatcher)
    {
        _dbContext = dbContext;
        _webhookDispatcher = webhookDispatcher;
    }

    [HttpPost("subscribtions")]
    public IActionResult CreateSubscription([FromBody] CreateWebhookRequest request)
    {
        WebhookSubscription newSubscription = new()
        {
            WebhookUrl = request.WebhookUrl,
            EventType = request.EventType,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.WebhookSubscriptions.Add(newSubscription);
        _dbContext.SaveChanges();

        return Ok(newSubscription);
    }

    [HttpPost("publish")]
    public async Task<IActionResult> PublishEvent([FromBody] PublishWebhookRequest request)
    {
        await _webhookDispatcher.DispatchAsync(request.EventType, request.Payload);
        return Ok();
    }
}

public record PublishWebhookRequest(string EventType, object Payload);