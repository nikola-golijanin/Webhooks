using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using Webhooks.API.Dtos;
using Webhooks.Contracts;

namespace Webhooks.API.IntegrationTests;

public class WebhooksControllerTests : BaseIntegrationTest
{
    private readonly CustomWebAppFactory _factory;

    public WebhooksControllerTests(CustomWebAppFactory factory) : base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateSubscription_ReturnsOkResult()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new CreateWebhookRequest("http://localhost:123","order.created");


        // Act
        var response = await client.PostAsJsonAsync("/api/webhooks/subscribtions", request);

        // Assert
        response.EnsureSuccessStatusCode();
        var subscription = DbContext.WebhookSubscriptions.Single(w => w.WebhookUrl == "http://localhost:123");
        Assert.NotNull(subscription);
        Assert.Equal("order.created", subscription.EventType);
    }

    [Fact]
    public async Task PublishEvent_ShouldPublishMessageWithCorrectPayload()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new PublishWebhookRequest("order.created","testpayload");


        // Act
        var response = await client.PostAsJsonAsync("/api/webhooks/publish", request);


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