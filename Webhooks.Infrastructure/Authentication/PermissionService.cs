using Microsoft.EntityFrameworkCore;
using Webhooks.Domain.Models;
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