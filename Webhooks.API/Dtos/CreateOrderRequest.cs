namespace Webhooks.API.Dtos;

public sealed record CreateOrderRequest(string CustomerName, decimal Amount);