using Microsoft.AspNetCore.Mvc;
using Webhooks.Application.Authentication;
using Webhooks.Domain.Models;
using Webhooks.Domain.Shared;
using Webhooks.Infrastructure.Authentication;
using Permission = Webhooks.Domain.Enums.Permission;

namespace Webhooks.Api.Controllers;

[Route("api/[controller]")]
public class ProfilesController : ApiController
{
    private readonly IProfileManager _profileManager;
    private readonly ILogger<ProfilesController> _logger;

    public ProfilesController(
        IProfileManager profileManager,
        ILogger<ProfilesController> logger)
    {
        _profileManager = profileManager;
        _logger = logger;
    }

    [HttpGet]
    [HasPermission(Permission.ReadProfiles)]
    public async Task<IActionResult> GetProfilesAsync(CancellationToken cancellationToken)
    {
        Result<HashSet<Profile>> profilesResult = await _profileManager.GetProfilesAsync(cancellationToken);
        if (profilesResult.IsFailure)
        {
            _logger.LogError("Failed to get profiles. {ErrorCode}", profilesResult.Error.Code);
            return HandleFailure(profilesResult);
        }
        return Ok(profilesResult.Value);
    }

    [HttpGet("{userId:int}")]
    [HasPermission(Permission.ReadProfiles)]
    public async Task<IActionResult> GetUserProfilesAsync(int userId, CancellationToken cancellationToken)
    {
        var userProfilesResult = await _profileManager.GetUserProfilesAsync(userId, cancellationToken);
        if (userProfilesResult.IsFailure)
        {
            _logger.LogError("Failed to get profiles for user with id {UserId}. {ErrorCode}", userId, userProfilesResult.Error.Code);
            return HandleFailure(userProfilesResult);
        }
        return Ok(userProfilesResult.Value);
    }

    [HttpPost("{profileId:int}/assign/{userId:int}")]
    [HasPermission(Permission.AssignProfiles)]
    public async Task<IActionResult> AssignProfileToUserAsync(int profileId, int userId, CancellationToken cancellationToken)
    {
        Result assignProfileResult = await _profileManager.AssignProfileToUserAsync(profileId, userId, cancellationToken);
        if (assignProfileResult.IsFailure)
        {
            _logger.LogError("Failed to assign profile with id {ProfileId} to user with id {UserId}. {ErrorCode}", profileId, userId, assignProfileResult.Error.Code);
            return HandleFailure(assignProfileResult);
        }
        return NoContent();
    }

    //TODO remove profile from user
    // think about exposing permission crud operations for admin users
}