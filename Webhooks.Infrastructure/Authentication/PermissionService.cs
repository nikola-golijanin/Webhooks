using Microsoft.EntityFrameworkCore;
using Webhooks.Persistance;
using Webhooks.Persistance.Queries;

namespace Webhooks.Infrastructure.Authentication;

public class PermissionService
{
    private readonly WebhooksDbContext _context;

    public PermissionService(WebhooksDbContext context)
    {
        _context = context;
    }

    public async Task<HashSet<string>> GetPermissionsAsync(int userId)
    {
        var permissions = await _context.Database
            .SqlQuery<string>(PermissionQueries.GetUserPermissions(userId))
            .ToHashSetAsync();

        return permissions;
    }
}