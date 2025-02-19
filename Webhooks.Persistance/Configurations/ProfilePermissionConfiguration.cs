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
        builder.HasData(AssignPermissions("Admin", Enum.GetValues<Domain.Enums.Permission>()));
        builder.HasData(AssignPermissions("OrderManager", Domain.Enums.Permission.CreateOrders, Domain.Enums.Permission.ReadOrders));
        builder.HasData(AssignPermissions("UserManager", Domain.Enums.Permission.ReadProfiles, Domain.Enums.Permission.AssignProfiles));
        builder.HasData(AssignPermissions("Subscriber", Domain.Enums.Permission.CreateSubscriptions));

    }

    private static IEnumerable<ProfilePermission> AssignPermissions(string profileName, params Domain.Enums.Permission[] permissions)
    {
        var profile = ProfileConfiguration
                    .GetDefaultProfiles()
                    .FirstOrDefault(r => r.Name == profileName);
        return permissions.Select(p => Create(p, profile!.Id));
    }

    private static ProfilePermission Create(Domain.Enums.Permission permission, int profileId) =>
    new()
    {
        ProfileId = profileId,
        PermissionId = (int)permission
    };
}
