using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Webhooks.Domain.Models;

namespace Webhooks.Persistance.Configurations;

public class WebhookDeliveryAttemptConfiguration : IEntityTypeConfiguration<WebhookDeliveryAttempt>
{
    public void Configure(EntityTypeBuilder<WebhookDeliveryAttempt> builder)
    {
        builder.ToTable("delivery_attempts", "webhooks");
        builder.HasKey(ws => ws.Id);

        builder.HasOne<WebhookSubscription>()
            .WithMany()
            .HasForeignKey(wda => wda.WebhookSubscriptionId);
    }
}