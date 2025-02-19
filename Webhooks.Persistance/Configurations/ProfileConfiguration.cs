using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Webhooks.Domain.Models;

namespace Webhooks.Persistance.Configurations;

public class ProfileConfiguration : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> builder)
    {
        builder.ToTable("profiles");

        builder.HasKey(r => r.Id);

        builder.HasMany(r => r.Permissions)
            .WithMany()
            .UsingEntity<ProfilePermission>();

        builder.HasMany(r => r.Users)
            .WithMany(u => u.Profiles)
            .UsingEntity(e => e.ToTable("profiles_users"));

        builder.HasData(GetDefaultProfiles());
    }

    internal static IEnumerable<Profile> GetDefaultProfiles()
    {
        return
        [
            new Profile{ Id = 1,Name = "Admin"},
            new Profile{ Id = 2,Name = "User"}
        ];
    }
}
