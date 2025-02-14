namespace Webhooks.Domain.Models;

public record Permission(int Id, string Name)
{
    public static readonly Permission AccessOrders = new(1, "AccessOrders");
    public static readonly Permission ReadOrders = new(2, "ReadOrders");
};