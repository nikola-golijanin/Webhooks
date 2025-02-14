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
                p.""Name""
            FROM permissions p 
            INNER JOIN role_permissions rp ON 
                p.""Id"" = rp.""PermissionId"" 
            INNER JOIN role_users ru ON 
                rp.""RoleId"" = ru.""RolesId"" 
            WHERE ru.""UsersId"" = {userId}")
            .ToHashSetAsync();

        return permissions;
    }
}