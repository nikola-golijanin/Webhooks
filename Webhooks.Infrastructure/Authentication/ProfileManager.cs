using Microsoft.EntityFrameworkCore;
using Webhooks.Application.Authentication;
using Webhooks.Domain.Errors;
using Webhooks.Domain.Models;
using Webhooks.Domain.Shared;
using Webhooks.Persistance;

namespace Webhooks.Infrastructure.Authentication;

public class ProfileManager : IProfileManager
{
    private readonly WebhooksDbContext _context;

    public ProfileManager(WebhooksDbContext context)
    {
        _context = context;
    }

    public async Task<Result> AssignProfileToUserAsync(int roleId, int userId, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Profiles)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
            return Result.Failure(DomainErrors.User.UserNotFound(userId));

        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

        if (role is null)
            return Result.Failure(DomainErrors.Profile.ProfileNotFound(roleId));

        user.Profiles.Add(role);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<HashSet<Profile>>> GetProfilesAsync(CancellationToken cancellationToken)
    {
        var roles = await _context.Roles
            .ToHashSetAsync(cancellationToken);

        return roles.Count == 0
            ? Result.Failure<HashSet<Profile>>(DomainErrors.Profile.NoProfilesFound)
            : Result.Success(roles);
    }

    public async Task<Result<HashSet<Profile>>> GetUserProfilesAsync(int userId, CancellationToken cancellationToken)
    {
        var userRoles = await _context.Users
            .Where(u => u.Id == userId)
            .SelectMany(u => u.Profiles)
            .ToHashSetAsync(cancellationToken);

        return userRoles.Count == 0
            ? Result.Failure<HashSet<Profile>>(DomainErrors.Profile.NoProfilesForUserFound(userId))
            : Result.Success(userRoles);
    }
}