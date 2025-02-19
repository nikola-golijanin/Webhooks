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

        var defaultProfiles = ProfileConfiguration.GetDefaultProfiles();

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
        builder.HasMany(u => u.Profiles)
            .WithMany(r => r.Users)
            .UsingEntity<Dictionary<string, object>>(
                "profiles_users",
                r => r.HasOne<Profile>().WithMany().HasForeignKey("profile_id"),
                u => u.HasOne<User>().WithMany().HasForeignKey("user_id"),
                je =>
                {
                    je.HasKey("profile_id", "user_id");
                    // Seed the join table data
                    je.HasData(
                        defaultProfiles.Select(profile => new
                        {
                            user_id = 1,
                            profile_id = profile.Id
                        })
                    );
                }
            );
    }
}
