using Microsoft.EntityFrameworkCore;
using Webhooks.Domain.Models;

namespace Webhooks.Persistance.Queries;

public static class ProfileQueries
{
    public static async Task<HashSet<Profile>> FindProfilesNotInList(this IQueryable<Profile> query, IEnumerable<int> profileIds, CancellationToken cancellationToken) =>
        await query
            .Where(p => !profileIds.Contains(p.Id))
            .ToHashSetAsync(cancellationToken);
            
    public static async Task<Profile?> GetProfileByIdAsync(this IQueryable<Profile> query, int profileId, CancellationToken cancellationToken) =>
        await query.FirstOrDefaultAsync(p => p.Id == profileId, cancellationToken);

    public static async Task<Profile?> GetDefaultProfileAsync(this IQueryable<Profile> query, CancellationToken cancellationToken) =>
        await query.FirstOrDefaultAsync(p => p.Name == "Subscriber", cancellationToken);

    public static async Task<HashSet<Profile>> GetUserProfilesAsync(this IQueryable<User> query, int userId, CancellationToken cancellationToken) =>
            await query.Where(u => u.Id == userId)
                .SelectMany(u => u.Profiles)
                .ToHashSetAsync(cancellationToken);
    
    public static async Task<HashSet<Profile>> GetProfilesAsync(this IQueryable<Profile> query, CancellationToken cancellationToken) =>
            await query.ToHashSetAsync(cancellationToken);
}