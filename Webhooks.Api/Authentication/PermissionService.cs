using Microsoft.EntityFrameworkCore;
using Webhooks.Api.Data;
using Webhooks.Api.Models;

namespace Webhooks.Api.Authentication;

public class PermissionService
{
    private readonly WebhooksDbContext _context;

    public PermissionService(WebhooksDbContext context)
    {
        _context = context;
    }

    public async Task<HashSet<string>> GetPermissionsAsync(Guid userId)
    {
        ICollection<Role>[] roles = await _context.Users.Include(u => u.Roles)
            .ThenInclude(r => r.Permissions)
            .Where(u => u.Id == userId)
            .Select(u => u.Roles)
            .ToArrayAsync();

        return roles.SelectMany(x => x)
            .SelectMany(x => x.Permissions)
            .Select(x => x.Name)
            .ToHashSet();
    }
}