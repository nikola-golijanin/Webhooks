using Microsoft.AspNetCore.Mvc;
using Webhooks.Api.Contracts.Profiles;
using Webhooks.Application.Authentication;
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
    [ProducesResponseType(typeof(IEnumerable<GetProfilesResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Get all profiles.")]
    [EndpointDescription("Retrieves all profiles.")]
    public async Task<IActionResult> GetProfilesAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all profiles.");
        var profilesResult = await _profileManager.GetProfilesAsync(cancellationToken);
        if (profilesResult.IsFailure)
        {
            _logger.LogError("Failed to get profiles. {ErrorCode}", profilesResult.Error.Code);
            return HandleFailure(profilesResult);
        }

        _logger.LogInformation("Successfully retrieved profiles.");
        var profiles = GetProfilesResponse.FromProfiles(profilesResult.Value);
        return Ok(profiles);
    }

    [HttpGet("{userId:int}")]
    [HasPermission(Permission.ReadProfiles)]
    [ProducesResponseType(typeof(IEnumerable<GetProfilesResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Get profiles for a user.")]
    [EndpointDescription("Retrieves profiles for the specified user ID.")]
    public async Task<IActionResult> GetUserProfilesAsync([FromRoute] int userId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting profiles for user with id {UserId}.", userId);
        var userProfilesResult =
            await _profileManager.GetUserProfilesAsync(userId, cancellationToken);

        if (userProfilesResult.IsFailure)
        {
            _logger.LogError("Failed to get profiles for user with id {UserId}. {ErrorCode}", userId,
                userProfilesResult.Error.Code);
            return HandleFailure(userProfilesResult);
        }

        _logger.LogInformation("Successfully retrieved profiles for user with id {UserId}.", userId);
        var profiles = GetProfilesResponse.FromProfiles(userProfilesResult.Value);
        return Ok(profiles);
    }

    [HttpPost("{profileId:int}/assign/{userId:int}")]
    [HasPermission(Permission.AssignProfiles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Assign a profile to a user.")]
    [EndpointDescription("Assigns the specified profile to the specified user.")]
    public async Task<IActionResult> AssignProfileToUserAsync([FromRoute] int profileId, [FromRoute] int userId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Assigning profile with id {ProfileId} to user with id {UserId}.", profileId, userId);
        var assignProfileResult =
            await _profileManager.AssignProfileToUserAsync(profileId, userId, cancellationToken);
        if (assignProfileResult.IsSuccess)
        {
            _logger.LogInformation("Successfully assigned profile with id {ProfileId} to user with id {UserId}.",
                profileId, userId);
            return NoContent();
        }

        _logger.LogError("Failed to assign profile with id {ProfileId} to user with id {UserId}. {ErrorCode}",
            profileId, userId, assignProfileResult.Error.Code);
        return HandleFailure(assignProfileResult);
    }

    [HttpGet("{userId:int}/not-contained")]
    [HasPermission(Permission.ReadProfiles)]
    [ProducesResponseType(typeof(IEnumerable<GetProfilesResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Get profiles not contained by a user.")]
    [EndpointDescription("Retrieves profiles that are not contained by the specified user.")]
    public async Task<IActionResult> GetProfilesUserDoesNotContainAsync([FromRoute] int userId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting profiles not contained by user with id {UserId}.", userId);
        var profilesResult =
            await _profileManager.GetProfilesUserDoesNotContainAsync(userId, cancellationToken);

        if (profilesResult.IsFailure)
        {
            _logger.LogError("Failed to get profiles user with id {UserId} does not contain. {ErrorCode}", userId,
                profilesResult.Error.Code);
            return HandleFailure(profilesResult);
        }

        _logger.LogInformation("Successfully retrieved profiles not contained by user with id {UserId}.", userId);
        var profiles = GetProfilesResponse.FromProfiles(profilesResult.Value);
        return Ok(profiles);
    }

    [HttpDelete("{profileId:int}/remove/{userId:int}")]
    [HasPermission(Permission.AssignProfiles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Remove a profile from a user.")]
    [EndpointDescription("Removes the specified profile from the specified user.")]
    public async Task<IActionResult> RemoveProfileFromUserAsync([FromRoute] int profileId, [FromRoute] int userId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Removing profile with id {ProfileId} from user with id {UserId}.", profileId, userId);
        var deleteProfileFromUserResult =
            await _profileManager.RemoveProfileFromUserAsync(profileId, userId, cancellationToken);

        if (deleteProfileFromUserResult.IsSuccess)
        {
            _logger.LogInformation("Successfully removed profile with id {ProfileId} from user with id {UserId}.",
                profileId, userId);
            return NoContent();
        }

        _logger.LogError("Failed to remove profile with id {ProfileId} from user with id {UserId}. {ErrorCode}",
            profileId, userId, deleteProfileFromUserResult.Error.Code);
        return HandleFailure(deleteProfileFromUserResult);
    }
}