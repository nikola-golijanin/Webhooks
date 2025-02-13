namespace Webhooks.Api.Models;

public record RoleUser
{
    public int RoleId { get; set; }
    public int UserId { get; set; }
}