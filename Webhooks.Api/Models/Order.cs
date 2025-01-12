namespace Webhooks.Api.Models;

public sealed record Order(Guid Id, string CustomerName, decimal Amount, DateTime CreatedAt);
