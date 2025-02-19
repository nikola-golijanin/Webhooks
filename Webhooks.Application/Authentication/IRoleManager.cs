using System;
using Webhooks.Domain.Models;
using Webhooks.Domain.Shared;

namespace Webhooks.Application.Authentication;

public interface IRoleManager
{
    Task<Result<HashSet<Profile>>> GetRolesAsync(CancellationToken cancellationToken);
    Task<Result<HashSet<Profile>>> GetUserRolesAsync(int userId, CancellationToken cancellationToken);
    Task<Result> AssignRoleToUserAsync(int roleId, int userId, CancellationToken cancellationToken);
}

