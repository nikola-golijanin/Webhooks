using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using Webhooks.API.Dtos;
using Webhooks.API.Models;
using Webhooks.API.Repositories;
using Webhooks.Contracts;

namespace Webhooks.API.IntegrationTests;

public class OrdersControllerTests : BaseIntegrationTest
{
    private readonly CustomWebAppFactory _factory;

    public OrdersControllerTests(CustomWebAppFactory factory) : base(factory)
    {
        _factory = factory;
    }


    [Fact]
    public async Task GetOrders_ReturnsOkResult()
    {
        // Arrange
        var client = _factory.CreateClient();

        var orders = new[]
        {
            new Order(1, "Customer A", 50.00m, DateTime.UtcNow.AddHours(-2)),
            new Order(2, "Customer B", 75.50m, DateTime.UtcNow.AddHours(-1)),
            new Order(3, "Customer C", 120.00m, DateTime.UtcNow)
        };

        foreach (var order in orders)
        {
            InMemoryOrderRepository.Add(order);
        }

        // Act
        var response = await client.GetAsync("/api/orders");

        // Assert
        response.EnsureSuccessStatusCode();
        var returnedOrders = await response.Content.ReadFromJsonAsync<List<Order>>();
        Assert.NotNull(returnedOrders);
        Assert.Equal(3, returnedOrders.Count);
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldPublishMessageWithCorrectPayload()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new CreateOrderRequest("Jane Smith", 150.00m);


        // Act
        var response = await client.PostAsJsonAsync("/api/orders", request);

        // Assert
        response.EnsureSuccessStatusCode();
        var harness = _factory.Services.GetRequiredService<ITestHarness>();

        // Wait for the message
        Assert.True(await harness.Published.Any<WebhookDispatched>());

        // Verify message content
        var message = harness.Published.Select<WebhookDispatched>().Single();
        var webhookDispatched = message.Context.Message;

        Assert.Equal("order.created", webhookDispatched.EventType);
        Assert.NotNull(webhookDispatched.Data);
    }
}