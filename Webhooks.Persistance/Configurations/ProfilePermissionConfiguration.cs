using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Webhooks.Domain.Models;

namespace Webhooks.Persistance.Configurations;

public class ProfilePermissionConfiguration : IEntityTypeConfiguration<ProfilePermission>
{
    public void Configure(EntityTypeBuilder<ProfilePermission> builder)
    {
        builder.ToTable("profiles_permissions");
        builder.HasKey(rp => new { rp.ProfileId, rp.PermissionId });
        builder.HasData(AssignAllPermissionsToAdminRole());
    }

    private static IEnumerable<ProfilePermission> AssignAllPermissionsToAdminRole()
    {
        var adminProfile = ProfileConfiguration
                    .GetDefaultProfiles()
                    .FirstOrDefault(r => r.Name == "Admin");
        var permissions = Enum.GetValues<Domain.Enums.Permission>();
        return permissions.Select(Create);

        ProfilePermission Create(Domain.Enums.Permission permission) =>
            new()
            {
                ProfileId = adminProfile!.Id,
                PermissionId = (int)permission
            };
    }
}
