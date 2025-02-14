using Microsoft.EntityFrameworkCore;
using Webhooks.Domain.Enums;
using Webhooks.Domain.Models;
using Permission = Webhooks.Domain.Models.Permission;

namespace Webhooks.Persistance;

public sealed class WebhooksDbContext : DbContext
{
    public WebhooksDbContext(DbContextOptions<WebhooksDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<WebhookSubscription> WebhookSubscriptions { get; set; }
    public DbSet<WebhookDeliveryAttempt> WebhookDeliveryAttempts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //roles
        modelBuilder.Entity<Role>(builder =>
        {
            builder.ToTable("roles");

            builder.HasKey(r => r.Id);

            builder.HasMany(r => r.Permissions)
                .WithMany()
                .UsingEntity<RolePermission>();

            builder.HasMany(r => r.Users)
                .WithMany(u => u.Roles)
                .UsingEntity(e => e.ToTable("role_users"));
        });

        //permissions
        modelBuilder.Entity<Permission>(builder =>
        {
            builder.ToTable("permissions");
            builder.HasKey(p => p.Id);

            var permissions = Enum.GetValues<Domain.Enums.Permission>()
                .Select(p => new Permission(Id: (int)p, Name: p.ToString()));

            builder.HasData(permissions);
        });

        //role permissions
        modelBuilder.Entity<RolePermission>(builder =>
        {
            builder.ToTable("role_permissions");
            builder.HasKey(rp => new { rp.RoleId, rp.PermissionId });
        });

        //orders
        modelBuilder.Entity<Order>(builder =>
        {
            builder.ToTable("orders");
            builder.HasKey(o => o.Id);
        });

        //users
        modelBuilder.Entity<User>(builder =>
        {
            builder.ToTable("users");
            builder.HasKey(u => u.Id);
        });

        //webhooks
        modelBuilder.Entity<WebhookSubscription>(builder =>
        {
            builder.ToTable("subscriptions", "webhooks");
            builder.HasKey(ws => ws.Id);
        });

        //webhook delivery attempts
        modelBuilder.Entity<WebhookDeliveryAttempt>(builder =>
        {
            builder.ToTable("delivery_attempts", "webhooks");
            builder.HasKey(ws => ws.Id);

            builder.HasOne<WebhookSubscription>()
                .WithMany()
                .HasForeignKey(wda => wda.WebhookSubscriptionId);
        });
        return;

        //TODO check this for seeding data
        static RolePermission Create(Role role, Domain.Enums.Permission permission)
        {
            return new RolePermission()
            {
                RoleId = role.Id,
                PermissionId = (int)permission
            };
        }
    }
}
