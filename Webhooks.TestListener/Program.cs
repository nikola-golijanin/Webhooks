var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<WebhookStore>();
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Receive a webhook delivery
app.MapPost("/webhooks", static (WebhookPayload payload, WebhookStore store, ILogger<Program> logger) =>
{
    store.Add(payload);
    logger.LogInformation("Received webhook: EventType={EventType}, Id={Id}, SubscriptionId={SubscriptionId}, Data={@Data}",
        payload.EventType, payload.Id, payload.SubscriptionId, payload.Data);
    return Results.Ok();
});

// Inspect received webhooks
app.MapGet("/webhooks", (WebhookStore store) => Results.Ok(store.GetAll()));

// Summary stats (count + last received timestamp)
app.MapGet("/stats", (WebhookStore store) =>
{
    var all = store.GetAll();
    return Results.Ok(new
    {
        all.Count,
        LastReceivedAt = all.Count > 0 ? all[^1].ReceivedAt : (DateTime?)null
    });
});

// Reset state between load test runs
app.MapDelete("/webhooks", (WebhookStore store) =>
{
    store.Clear();
    return Results.NoContent();
});

app.Run();
