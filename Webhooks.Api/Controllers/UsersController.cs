using Microsoft.AspNetCore.Mvc;
using Webhooks.Api.Contracts.Users;
using Webhooks.Application.Users;
using Webhooks.Domain.Shared;

namespace Webhooks.Api.Controllers;

[Route("api/[controller]")]
public class UsersController : ApiController
{
    private readonly IUserService _userService;

    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginUserAsync([FromBody] LoginUserRequest request, CancellationToken cancellationToken)
    {
        Result<string> tokenResult = await _userService.LoginAsync(request.Email, cancellationToken);

        if (tokenResult.IsFailure)
        {
            _logger.LogError("Failed to login user with {Email}. {ErrorCode}", request.Email, tokenResult.Error.Code);
            return HandleFailure(tokenResult);
        }

        return Ok(tokenResult.Value);
    }

    //TODO register user
}