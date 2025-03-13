using Microsoft.Extensions.Logging;
using Webhooks.Application.Authentication;
using Webhooks.Domain.Errors;
using Webhooks.Domain.Models;
using Webhooks.Domain.Shared;
using Webhooks.Persistance;
using Webhooks.Persistance.Queries;

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
        var user = await _context.Users.GetUserWithProfilesAsync(userId, cancellationToken);

        if (user is null)
            return Result.Failure(DomainErrors.User.UserNotFound(userId));

        var role = await _context.Roles.GetRoleByIdAsync(roleId, cancellationToken);

        if (role is null)
            return Result.Failure(DomainErrors.Profile.ProfileNotFound(roleId));

        user.Profiles.Add(role);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<HashSet<Profile>>> GetProfilesAsync(CancellationToken cancellationToken)
    {
        var roles = await _context.Roles.GetRolesAsync(cancellationToken);

        return roles.Count == 0
            ? Result.Failure<HashSet<Profile>>(DomainErrors.Profile.NoProfilesFound)
            : Result.Success(roles);
    }

    public async Task<Result<HashSet<Profile>>> GetProfilesUserDoesNotContainAsync(int userId, CancellationToken cancellationToken)
    {
        var userWithProfileIds = await _context.Users.GetUserIdWithProfileIdsAsync(userId, cancellationToken);

        if (userWithProfileIds is null)
            return Result.Failure<HashSet<Profile>>(DomainErrors.User.UserNotFound(userId));

        var profilesThatUserDoesNotContain = await _context.Set<Profile>()
           .FindProfilesNotInList(userWithProfileIds.ProfileIds, cancellationToken);

        return profilesThatUserDoesNotContain.Count == 0
                ? Result.Failure<HashSet<Profile>>(DomainErrors.Profile.UserAssignedToAllProfiles(userId))
                : Result.Success(profilesThatUserDoesNotContain);
    }

    public async Task<Result<HashSet<Profile>>> GetUserProfilesAsync(int userId, CancellationToken cancellationToken)
    {
        var userProfiles = await _context.Users.GetUserProfilesAsync(userId, cancellationToken);

        return userProfiles.Count == 0
            ? Result.Failure<HashSet<Profile>>(DomainErrors.Profile.NoProfilesForUserFound(userId))
            : Result.Success(userProfiles);
    }

    public async Task<Result> RemoveProfileFromUserAsync(int profileId, int userId, CancellationToken cancellationToken)
    {
        var user = await _context.Users.GetUserWithProfilesAsync(userId, cancellationToken);

        if (user is null)
            return Result.Failure(DomainErrors.User.UserNotFound(userId));

        var profileToRemove = user.Profiles.FirstOrDefault(p => p.Id == profileId);

        if (profileToRemove is null)
            return Result.Failure(DomainErrors.Profile.ProfileNotFound(profileId));

        user.Profiles.Remove(profileToRemove);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}