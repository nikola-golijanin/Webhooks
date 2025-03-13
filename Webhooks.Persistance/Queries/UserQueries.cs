using Microsoft.EntityFrameworkCore;
using Webhooks.Domain.Models;
using Webhooks.Persistance.QueryProjections;

namespace Webhooks.Persistance.Queries;

public static class UserQueries
{
    public static async Task<User?> FindByEmailAsync(this IQueryable<User> query, string email, CancellationToken cancellationToken) =>
           await query.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public static async Task<bool> IsEmailRegisteredAsync(this IQueryable<User> query, string email, CancellationToken cancellationToken) =>
        await query.AnyAsync(u => u.Email == email, cancellationToken);

    public static async Task<User?> GetUserWithProfilesAsync(this IQueryable<User> query, int userId, CancellationToken cancellationToken) =>
        await query.Include(u => u.Profiles)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

    public static async Task<UserIdWithProfileIds?> GetUserIdWithProfileIdsAsync(this IQueryable<User> query, int userId, CancellationToken cancellationToken) =>
        await query.Where(u => u.Id == userId)
            .Select(u => new UserIdWithProfileIds
            (
                u.Id,
                u.Profiles.Select(p => p.Id).ToList()
            ))
            .FirstOrDefaultAsync(cancellationToken);
}
