using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
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
        await db.Database.MigrateAsync();
    }

    /// <summary>
    ///     Configures and adds the Scalar UI for API reference documentation.
    ///     This provides a user-friendly interface to explore the Webhooks API.
    /// </summary>
    /// <param name="app">The WebApplication instance.</param>
    public static void AddScalarUi(this WebApplication app)
    {
        app.MapScalarApiReference(
            options =>
            {
                options.Title = "Webhooks API";
                options.DotNetFlag = true;
            }
        );
    }

    /// <summary>
    ///     Checks if the current hosting environment name is  Docker.
    /// </summary>
    /// <param name="hostEnvironment">An instance of IHostEnvironment.</param>
    /// <returns>True if the environment name is Docker, otherwise false.</returns>
    public static bool IsDocker(this IWebHostEnvironment hostEnvironment) =>
        "Docker".Equals(hostEnvironment.EnvironmentName, StringComparison.OrdinalIgnoreCase);
   
}