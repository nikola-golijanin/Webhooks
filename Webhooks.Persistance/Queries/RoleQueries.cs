using Microsoft.EntityFrameworkCore;
using Webhooks.Domain.Models;

namespace Webhooks.Persistance.Queries;

public static class RoleQueries
{
    public static async Task<HashSet<Profile>> GetRolesAsync(this IQueryable<Profile> query, CancellationToken cancellationToken) =>
        await query.ToHashSetAsync(cancellationToken);

    public static async Task<Profile?> GetRoleByIdAsync(this IQueryable<Profile> query, int roleId, CancellationToken cancellationToken) =>
        await query.FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);
}
