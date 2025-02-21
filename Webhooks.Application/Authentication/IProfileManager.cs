using Webhooks.Domain.Models;
using Webhooks.Domain.Shared;

namespace Webhooks.Application.Authentication;

public interface IProfileManager
{
    Task<Result<HashSet<Profile>>> GetProfilesAsync(CancellationToken cancellationToken);
    Task<Result<HashSet<Profile>>> GetUserProfilesAsync(int userId, CancellationToken cancellationToken);
    Task<Result> AssignProfileToUserAsync(int roleId, int userId, CancellationToken cancellationToken);
    Task<Result<HashSet<Profile>>> GetProfilesUserDoesNotContainAsync(int userId, CancellationToken cancellationToken);
}