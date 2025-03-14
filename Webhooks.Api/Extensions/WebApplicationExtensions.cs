using Microsoft.EntityFrameworkCore;
using Webhooks.Persistance;

namespace Webhooks.Api.Extensions;

public static class WebApplicationExtensions
{
    /// <summary>
    ///     Applies pending database migrations asynchronously at application startup.
    ///     Ensures the database schema is up-to-date before handling requests.
    /// </summary>
    /// <param name="app">The WebApplication instance.</param>
    public static async Task ApplyMigrationAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WebhooksDbContext>();

        var hasPendingMigrations = db.Database.GetPendingMigrations().Any();

        if (hasPendingMigrations)
            await db.Database.MigrateAsync();
    }


    /// <summary>
    ///     Checks if the current hosting environment name is  Docker.
    /// </summary>
    /// <param name="hostEnvironment">An instance of IHostEnvironment.</param>
    /// <returns>True if the environment name is Docker, otherwise false.</returns>
    public static bool IsDocker(this IWebHostEnvironment hostEnvironment) =>
        "Docker".Equals(hostEnvironment.EnvironmentName, StringComparison.OrdinalIgnoreCase);
}