using Microsoft.EntityFrameworkCore;
using Webhooks.Api.Models;

namespace Webhooks.Api.Data;

public sealed class WebhooksDbContext : DbContext
{
    public WebhooksDbContext(DbContextOptions<WebhooksDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Order> Orders { get; set; }
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

            builder.HasData(Role.Registered);
        });
        
        //permissions
        modelBuilder.Entity<Permission>(builder =>
        {
            builder.ToTable("permissions");
            builder.HasKey(p => p.Id);

            var permissions = Enum.GetValues<Authentication.Permission>()
                .Select(p => new Permission(Id: (int)p, Name: p.ToString()));
            
            builder.HasData(permissions);
        });
        
        //role permissions
        modelBuilder.Entity<RolePermission>(builder =>
        {
            builder.ToTable("role_permissions");
            builder.HasKey(rp => new { rp.RoleId, rp.PermissionId });
            builder.HasData(Create(Role.Registered,Authentication.Permission.ReadOrders));
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

        static RolePermission Create(Role role, Authentication.Permission permission)
        {
            return new RolePermission()
            {
                RoleId = role.Id,
                PermissionId = (int)permission
            };
        }
    }
}
