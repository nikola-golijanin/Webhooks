using Microsoft.EntityFrameworkCore;
using Webhooks.API.Data;

namespace Webhooks.API.Extensions;

public static class WebAppExtensions
{
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WebhooksDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}
