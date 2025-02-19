using Microsoft.EntityFrameworkCore;
using Webhooks.Persistance;

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
            .SqlQuery<string>($@"
            SELECT 
                p.Name
            FROM permissions p 
            INNER JOIN profiles_permissions pp ON 
                p.id = pp.permission_id 
            INNER JOIN profiles_users pu  ON 
                pp.profile_id = pu.profile_id 
            WHERE pu.user_id = {userId}")
            .ToHashSetAsync();

        return permissions;
    }
}