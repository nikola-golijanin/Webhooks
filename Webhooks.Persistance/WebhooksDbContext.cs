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
    public DbSet<Profile> Roles { get; set; }
    public DbSet<WebhookSubscription> WebhookSubscriptions { get; set; }
    public DbSet<WebhookDeliveryAttempt> WebhookDeliveryAttempts { get; set; }

    // This will pick up all the configurations from the assembly. All confs are in /Configurations
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(AssemblyReference.Assembly);
}
