using Microsoft.AspNetCore.Mvc;
using Webhooks.Api.Models;
using Webhooks.Api.Repositories;
using Webhooks.Api.Services;

namespace Webhooks.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly InMemoryOrderRepository _orderRepository;
    private readonly WebhookDispatcher _webhookDispatcher;
    public OrdersController(InMemoryOrderRepository orderRepository, WebhookDispatcher webhookDispatcher)
    {
        _orderRepository = orderRepository;
        _webhookDispatcher = webhookDispatcher;
    }
    [HttpGet]
    public IActionResult GetAllOrders()
    {
        var orders = _orderRepository.GetAll();
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

        _orderRepository.Add(order);

        await _webhookDispatcher.DispatchAsync("order.created", order);
        return Ok(order);
    }
}
