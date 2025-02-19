using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Webhooks.Application.Authentication;
using Webhooks.Domain.Models;
using Webhooks.Domain.Shared;
using Webhooks.Infrastructure.Authentication;
using Permission = Webhooks.Domain.Enums.Permission;

namespace Webhooks.Api.Controllers;

[Route("api/[controller]")]
public class RolesController : ApiController
{
    private readonly IRoleManager _roleManager;
    private readonly ILogger<RolesController> _logger;

    public RolesController(
        IRoleManager roleManager,
        ILogger<RolesController> logger)
    {
        _roleManager = roleManager;
        _logger = logger;
    }

    [HttpGet]
    [HasPermission(Permission.ReadRoles)]
    public async Task<IActionResult> GetRolesAsync(CancellationToken cancellationToken)
    {
        Result<HashSet<Profile>> getRolesResult = await _roleManager.GetRolesAsync(cancellationToken);
        if (getRolesResult.IsFailure)
        {
            _logger.LogError("Failed to get roles. {ErrorCode}", getRolesResult.Error.Code);
            return HandleFailure(getRolesResult);
        }
        return Ok(getRolesResult.Value);
    }

    [HttpGet("{userId:int}")]
    [HasPermission(Permission.ReadRoles)]
    public async Task<IActionResult> GetUserRolesAsync(int userId, CancellationToken cancellationToken)
    {
        var userRolesResult = await _roleManager.GetUserRolesAsync(userId, cancellationToken);
        if (userRolesResult.IsFailure)
        {
            _logger.LogError("Failed to get roles for user with id {UserId}. {ErrorCode}", userId, userRolesResult.Error.Code);
            return HandleFailure(userRolesResult);
        }
        return Ok(userRolesResult.Value);
    }

    [HttpPost("{roleId:int}/assign/{userId:int}")]
    //[HasPermission(Permission.AssignRoles)]
    public async Task<IActionResult> AssignRoleToUserAsync(int roleId, int userId, CancellationToken cancellationToken)
    {
        Result assignRoleResult = await _roleManager.AssignRoleToUserAsync(roleId, userId, cancellationToken);
        if (assignRoleResult.IsFailure)
        {
            _logger.LogError("Failed to assign role with id {RoleId} to user with id {UserId}. {ErrorCode}", roleId, userId, assignRoleResult.Error.Code);
            return HandleFailure(assignRoleResult);
        }
        return NoContent();
    }
}