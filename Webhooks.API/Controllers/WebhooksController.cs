using Microsoft.AspNetCore.Mvc;
using Webhooks.API.Data;
using Webhooks.API.Models;

namespace Webhooks.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WebhooksController : ControllerBase
{
    private readonly WebhooksDbContext _dbContext;

    public WebhooksController(WebhooksDbContext dbContext)
    {
        _dbContext = dbContext;
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
}
