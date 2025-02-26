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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProfilesAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all profiles.");
        Result<HashSet<Profile>> profilesResult = await _profileManager.GetProfilesAsync(cancellationToken);
        if (profilesResult.IsFailure)
        {
            _logger.LogError("Failed to get profiles. {ErrorCode}", profilesResult.Error.Code);
            return HandleFailure(profilesResult);
        }

        _logger.LogInformation("Successfully retrieved profiles.");
        return Ok(profilesResult.Value);
    }

    [HttpGet("{userId:int}")]
    [HasPermission(Permission.ReadProfiles)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUserProfilesAsync(int userId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting profiles for user with id {UserId}.", userId);
        var userProfilesResult = await _profileManager.GetUserProfilesAsync(userId, cancellationToken);
        if (userProfilesResult.IsSuccess)
        {
            _logger.LogInformation("Successfully retrieved profiles for user with id {UserId}.", userId);
            return Ok(userProfilesResult.Value);
        }

        _logger.LogError("Failed to get profiles for user with id {UserId}. {ErrorCode}", userId, userProfilesResult.Error.Code);
        return HandleFailure(userProfilesResult);
    }

    [HttpPost("{profileId:int}/assign/{userId:int}")]
    [HasPermission(Permission.AssignProfiles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AssignProfileToUserAsync(int profileId, int userId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Assigning profile with id {ProfileId} to user with id {UserId}.", profileId, userId);
        Result assignProfileResult =
            await _profileManager.AssignProfileToUserAsync(profileId, userId, cancellationToken);
        if (assignProfileResult.IsSuccess)
        {
            _logger.LogInformation("Successfully assigned profile with id {ProfileId} to user with id {UserId}.", profileId, userId);
            return NoContent();
        }

        _logger.LogError("Failed to assign profile with id {ProfileId} to user with id {UserId}. {ErrorCode}",
            profileId, userId, assignProfileResult.Error.Code);
        return HandleFailure(assignProfileResult);
    }


    [HttpGet("{userId:int}/not-contained")]
    [HasPermission(Permission.ReadProfiles)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProfilesUserDoesNotContainAsync(int userId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting profiles not contained by user with id {UserId}.", userId);
        Result<HashSet<Profile>> profilesResult =
            await _profileManager.GetProfilesUserDoesNotContainAsync(userId, cancellationToken);

        if (profilesResult.IsSuccess)
        {
            _logger.LogInformation("Successfully retrieved profiles not contained by user with id {UserId}.", userId);
            return Ok(profilesResult.Value);
        }


        _logger.LogError("Failed to get profiles user with id {UserId} does not contain. {ErrorCode}", userId,
            profilesResult.Error.Code);
        return HandleFailure(profilesResult);
    }

    [HttpDelete("{profileId:int}/remove/{userId:int}")]
    [HasPermission(Permission.AssignProfiles)]
    [ProducesResponseType(typeof(string), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveProfileFromUserAsync(int profileId, int userId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Removing profile with id {ProfileId} from user with id {UserId}.", profileId, userId);
        var deleteProfileFromUserResult = await _profileManager.RemoveProfileFromUserAsync(profileId, userId, cancellationToken);
        if (deleteProfileFromUserResult.IsSuccess)
        {
            _logger.LogInformation("Successfully removed profile with id {ProfileId} from user with id {UserId}.", profileId, userId);
            return NoContent();
        }

        _logger.LogError("Failed to remove profile with id {ProfileId} from user with id {UserId}. {ErrorCode}",
            profileId, userId, deleteProfileFromUserResult.Error.Code);
        return HandleFailure(deleteProfileFromUserResult);
    }
}