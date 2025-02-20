using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Webhooks.Domain.Models;

namespace Webhooks.Persistance.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("permissions");
        builder.HasKey(p => p.Id);

        var permissions = Enum.GetValues<Domain.Enums.Permission>()
            .Select(p => new Permission { Id = (int)p, Name = p.ToString() });

        builder.HasData(permissions);
    }
}