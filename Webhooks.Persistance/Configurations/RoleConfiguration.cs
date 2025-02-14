using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Webhooks.Domain.Models;

namespace Webhooks.Persistance.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");

        builder.HasKey(r => r.Id);

        builder.HasMany(r => r.Permissions)
            .WithMany()
            .UsingEntity<RolePermission>();

        builder.HasMany(r => r.Users)
            .WithMany(u => u.Roles)
            .UsingEntity(e => e.ToTable("role_users"));

        builder.HasData(GetDefaultRoles());
    }

    internal static IEnumerable<Role> GetDefaultRoles()
    {
        return
        [
            new Role(1, "Admin"),
            new Role(2, "User")
        ];
    }
}
