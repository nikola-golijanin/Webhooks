using Microsoft.AspNetCore.Mvc;
using Webhooks.API.Models;
using Webhooks.API.Repositories;
using Webhooks.API.Services;

namespace Webhooks.API.Controllers;

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

    [HttpPost]
    public async Task<IActionResult> CreateOrderAsync([FromBody] CreateOrderRequest request)
    {
        // In a real application, you would save the order to a database here.
        var newOrder = new Order(
            Id: new Random().Next(1, 1000),
            CustomerName: request.CustomerName,
            Amount: request.Amount,
            CreatedAt: DateTime.UtcNow
        );

        _orderRepository.Add(newOrder);
        await _webhookDispatcher.DispatchAsync("order.created", newOrder);

        return Ok(newOrder);
    }

    [HttpGet]
    public IActionResult GetAllOrders()
    {
        return Ok(_orderRepository.GetAll());
    }
}
