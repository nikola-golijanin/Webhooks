using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Webhooks.Api.Contracts.Orders;
using Webhooks.Api.Data;
using Webhooks.Api.Models;
using Webhooks.Api.Services.Publishers;

namespace Webhooks.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly WebhooksDbContext _context;
    private readonly WebhookDispatcher _webhookDispatcher;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        WebhooksDbContext context,
        WebhookDispatcher webhookDispatcher,
        ILogger<OrdersController> logger)
    {
        _context = context;
        _webhookDispatcher = webhookDispatcher;
        _logger = logger;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllOrders()
    {
        var orders = await _context.Orders.ToListAsync();
        return Ok(orders);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var order = new Order(
            Guid.NewGuid(),
            request.CustomerName,
            request.Amount,
            DateTime.UtcNow);

        _context.Orders.Add(order);

        await _context.SaveChangesAsync();
        _logger.LogInformation("Order created. {OrderId}, {CustomerName}, {Amount}", order.Id, order.CustomerName, order.Amount);

        await _webhookDispatcher.DispatchAsync("order.created", order);
        return Ok(order);
    }
}
