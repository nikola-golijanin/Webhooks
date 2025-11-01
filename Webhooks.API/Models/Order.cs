namespace Webhooks.API.Models;

public sealed record Order(long Id, string CustomerName, decimal Amount,DateTime CreatedAt);

public sealed record CreateOrderRequest(string CustomerName, decimal Amount);