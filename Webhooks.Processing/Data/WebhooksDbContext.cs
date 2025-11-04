using Microsoft.EntityFrameworkCore;
using Webhooks.Processing.Models;

namespace Webhooks.Processing.Data;

public class WebhooksDbContext : DbContext
{

    public WebhooksDbContext(DbContextOptions<WebhooksDbContext> options) : base(options)
    {
    }

    public DbSet<WebhookSubscription> WebhookSubscriptions { get; set; }
    public DbSet<WebhookDeliveryAttempt> WebhookDeliveryAttempts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WebhookSubscription>(builder =>
        {
            builder.ToTable("subscriptions", "webhooks");
            builder.HasKey(ws => ws.Id);
        });

        modelBuilder.Entity<WebhookDeliveryAttempt>(builder =>
        {
            builder.ToTable("delivery_attempts", "webhooks");
            builder.HasKey(wda => wda.Id);

            builder.HasOne<WebhookSubscription>()
                .WithMany()
                .HasForeignKey(wda => wda.WebhookSubscriptionId);
        });
    }
}
