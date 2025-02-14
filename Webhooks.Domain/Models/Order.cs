namespace Webhooks.Domain.Models;

public sealed record Order(Guid Id, string CustomerName, decimal Amount, DateTime CreatedAt);
