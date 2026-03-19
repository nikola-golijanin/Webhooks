using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Webhooks.API.Data;
using Webhooks.API.Dtos;
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
    public async Task<IActionResult> CreateSubscription([FromBody] CreateWebhookRequest request)
    {
        WebhookSubscription newSubscription = new()
        {
            WebhookUrl = request.WebhookUrl,
            EventType = request.EventType,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.WebhookSubscriptions.Add(newSubscription);
        await _dbContext.SaveChangesAsync();

        return Ok(newSubscription);
    }

    [HttpGet("subscriptions")]
    public async Task<IActionResult> GetSubscriptions()
    {
        return Ok(await _dbContext.WebhookSubscriptions.ToListAsync());
    }

    [HttpGet("delivery-attempts/{id}")]
    public async Task<IActionResult> GetDeliveryAttempts([FromQuery] bool? success, [FromRoute] int id)
    {
        var query = _dbContext.WebhookDeliveryAttempts.AsQueryable();
        if (success.HasValue)
            query = query
                .Where(d => d.Id == id)
                .Where(da => da.Success == success.Value);
        
        return Ok(await query.ToListAsync());
    }

    [HttpPost("publish")]
    public async Task<IActionResult> PublishEvent([FromBody] PublishWebhookRequest request)
    {
        await _webhookDispatcher.DispatchAsync(request.EventType, request.Payload);
        return Ok();
    }
}