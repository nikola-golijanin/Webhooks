using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Webhooks.Application.Authentication;
using Webhooks.Domain.Errors;
using Webhooks.Domain.Models;
using Webhooks.Domain.Shared;
using Webhooks.Infrastructure.QueryProjections;
using Webhooks.Persistance;

namespace Webhooks.Infrastructure.Authentication;

public class ProfileManager : IProfileManager
{
    private readonly WebhooksDbContext _context;
    private readonly ILogger<ProfileManager> _logger;

    public ProfileManager(WebhooksDbContext context, ILogger<ProfileManager> logger)
    {
        _context = context;
        _logger = logger;
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

    public async Task<Result<HashSet<Profile>>> GetProfilesUserDoesNotContainAsync(int userId, CancellationToken cancellationToken)
    {
        var userWithProfileIds = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => new UserIdWithProfileIds
            (
                u.Id,
                u.Profiles.Select(p => p.Id).ToList()
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (userWithProfileIds is null)
            return Result.Failure<HashSet<Profile>>(DomainErrors.User.UserNotFound(userId));

        (_, IEnumerable<int> profileIds) = userWithProfileIds;

        var profilesThatUserDoesNotContain = await _context.Set<Profile>()
            .Where(p => !profileIds.Contains(p.Id))
            .ToHashSetAsync(cancellationToken);

        return profilesThatUserDoesNotContain.Count == 0
                ? Result.Failure<HashSet<Profile>>(DomainErrors.Profile.UserAssignedToAllProfiles(userId))
                : Result.Success(profilesThatUserDoesNotContain);
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