namespace Webhooks.Api.Contracts.Orders;

public sealed record CreateOrderRequest(string CustomerName, decimal Amount);