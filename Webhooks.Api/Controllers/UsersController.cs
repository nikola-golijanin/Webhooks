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
    public async Task<IActionResult> LoginUserAsync([FromBody] LoginUserRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Attempting to login user with email {Email}.", request.Email);
        Result<string> tokenResult = await _userService.LoginAsync(request.Email, cancellationToken);

        if (tokenResult.IsFailure)
        {
            _logger.LogError("Failed to login user with {Email}. {ErrorCode}", request.Email, tokenResult.Error.Code);
            return HandleFailure(tokenResult);
        }

        _logger.LogInformation("User with email {Email} logged in successfully.", request.Email);
        return Ok(tokenResult.Value);
    }

    //TODO register user
    [HttpPost("register")]
    public async Task<IActionResult> RegisterUserAsync([FromBody] RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Attempting to register user with email {Email}.", request.Email);
        Result registerResult = await _userService.RegisterAsync(request.Email, cancellationToken);

        if (registerResult.IsFailure)
        {
            _logger.LogError("Failed to register user with {Email}. {ErrorCode}", request.Email, registerResult.Error.Code);
            return HandleFailure(registerResult);
        }
        _logger.LogInformation("User with email {Email} registered successfully.", request.Email);
        return NoContent();
    }
}
