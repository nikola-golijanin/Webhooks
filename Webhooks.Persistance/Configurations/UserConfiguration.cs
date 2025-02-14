using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Webhooks.Domain.Models;

namespace Webhooks.Persistance.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(u => u.Id);

        var defaultRoles = RoleConfiguration.GetDefaultRoles();

        //add one admin user
        builder.HasData(new User
        {
            Id = 1,
            Email = "admin@admin.com",
            FirstName = "Admin",
            LastName = "Admin",
            CreatedOnUtc = DateTime.SpecifyKind(new DateTime(2021, 1, 1, 0, 0, 0), DateTimeKind.Utc)
        });

        // Configure the many-to-many relationship
        builder.HasMany(u => u.Roles)
            .WithMany(r => r.Users)
            .UsingEntity<Dictionary<string, object>>(
                "role_users",
                r => r.HasOne<Role>().WithMany().HasForeignKey("RolesId"),
                u => u.HasOne<User>().WithMany().HasForeignKey("UsersId"),
                je =>
                {
                    je.HasKey("RolesId", "UsersId");
                    // Seed the join table data
                    je.HasData(
                        defaultRoles.Select(role => new
                        {
                            UsersId = 1,
                            RolesId = role.Id
                        })
                    );
                }
            );
    }
}
