using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Webhooks.Api.Contracts.Orders;
using Webhooks.Domain.Models;
using Webhooks.Infrastructure.Authentication;
using Webhooks.Infrastructure.Webhooks;
using Webhooks.Persistance;
using Permission = Webhooks.Domain.Enums.Permission;

namespace Webhooks.Api.Controllers;

[Route("api/[controller]")]
public class OrdersController : ApiController
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
    [HasPermission(Permission.ReadOrders)]
    public async Task<IActionResult> GetAllOrders()
    {
        var orders = await _context.Orders.ToListAsync();
        return Ok(orders);
    }

    [HttpPost]
    [HasPermission(Permission.CreateOrders)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var order = new Order
        {
            CustomerName = request.CustomerName,
            Amount =
            request.Amount,
            CreatedAtUtc = DateTime.UtcNow
        };
        _context.Orders.Add(order);

        await _context.SaveChangesAsync();
        _logger.LogInformation("Order created. {OrderId}, {CustomerName}, {Amount}", order.Id, order.CustomerName, order.Amount);

        await _webhookDispatcher.DispatchAsync("order.created", order);
        return Ok(order);
    }
}