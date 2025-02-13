namespace Webhooks.Api.Models;

public record RolePermission
{
    public int RoleId { get; set; }
    public int PermissionId { get; set; }
}