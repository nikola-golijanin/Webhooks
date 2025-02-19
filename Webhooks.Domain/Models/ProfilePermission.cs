using System.ComponentModel.DataAnnotations.Schema;

namespace Webhooks.Domain.Models;

public sealed class ProfilePermission
{
    [Column("profile_id")]
    public int ProfileId { get; set; }

    [Column("permission_id")]
    public int PermissionId { get; set; }
}