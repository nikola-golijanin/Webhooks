using System.ComponentModel.DataAnnotations.Schema;

namespace Webhooks.Domain.Models;

public sealed class Order
{
    [Column("id")]
    public int Id { get; init; }

    [Column("customer_name")]
    public string CustomerName { get; init; } = string.Empty;

    [Column("amount")]
    public decimal Amount { get; init; }

    [Column("created_at_utc")]
    public DateTime CreatedAtUtc { get; init; }
}
