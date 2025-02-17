using Microsoft.EntityFrameworkCore;
using Webhooks.Application.Authentication;
using Webhooks.Domain.Errors;
using Webhooks.Domain.Models;
using Webhooks.Domain.Shared;
using Webhooks.Persistance;

namespace Webhooks.Infrastructure.Authentication;

public class RoleManager : IRoleManager
{

    private readonly WebhooksDbContext _context;

    public RoleManager(WebhooksDbContext context)
    {
        _context = context;
    }

    public async Task<Result> AssignRoleToUserAsync(int roleId, int userId, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
            return Result.Failure(DomainErrors.User.UserNotFound(userId));

        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

        if (role is null)
            return Result.Failure(DomainErrors.Role.RoleNotFound(roleId));

        user.Roles.Add(role);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<HashSet<Role>>> GetRolesAsync(CancellationToken cancellationToken)
    {
        var roles = await _context.Roles
            .ToHashSetAsync(cancellationToken);

        if (roles.Count == 0)
            return Result.Failure<HashSet<Role>>(DomainErrors.Role.NoRolesFound);

        return Result.Success(roles);
    }

    public async Task<Result<HashSet<Role>>> GetUserRolesAsync(int userId, CancellationToken cancellationToken)
    {
        var userRoles = await _context.Users
                    .Where(u => u.Id == userId)
                    .SelectMany(u => u.Roles)
                    .ToHashSetAsync(cancellationToken);

        if (userRoles.Count == 0)
            return Result.Failure<HashSet<Role>>(DomainErrors.Role.NoRolesForUserFound(userId));

        return Result.Success(userRoles);
    }
}
