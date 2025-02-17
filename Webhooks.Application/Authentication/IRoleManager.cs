using System;
using Webhooks.Domain.Models;
using Webhooks.Domain.Shared;

namespace Webhooks.Application.Authentication;

public interface IRoleManager
{
    Task<Result<HashSet<Role>>> GetRolesAsync(CancellationToken cancellationToken);
    Task<Result<HashSet<Role>>> GetUserRolesAsync(int userId, CancellationToken cancellationToken);
    Task<Result> AssignRoleToUserAsync(int roleId, int userId, CancellationToken cancellationToken);
}

