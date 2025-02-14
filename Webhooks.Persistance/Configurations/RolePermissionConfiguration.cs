using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Webhooks.Domain.Models;

namespace Webhooks.Persistance.Configurations;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("role_permissions");
        builder.HasKey(rp => new { rp.RoleId, rp.PermissionId });
        builder.HasData(AssignAllPermissionsToAdminRole());
    }

    private static IEnumerable<RolePermission> AssignAllPermissionsToAdminRole()
    {
        var adminRole = RoleConfiguration
                    .GetDefaultRoles()
                    .FirstOrDefault(r => r.Name == "Admin");
        var permissions = Enum.GetValues<Domain.Enums.Permission>();
        return permissions.Select(Create);

        RolePermission Create(Domain.Enums.Permission permission) =>
            new()
            {
                RoleId = adminRole!.Id,
                PermissionId = (int)permission
            };
    }
}
