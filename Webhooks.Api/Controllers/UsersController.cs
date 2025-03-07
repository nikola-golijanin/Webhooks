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
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Login a user.")]
    [EndpointDescription("Attempts to login a user with the provided email.")]
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

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Register a new user.")]
    [EndpointDescription("Registers a new user with the provided email.")]
    public async Task<IActionResult> RegisterUserAsync([FromBody] RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Attempting to register user with email {Email}.", request.Email);
        var registerResult = await _userService.RegisterAsync(request.Email, cancellationToken);

        if (registerResult.IsFailure)
        {
            _logger.LogError("Failed to register user with {Email}. {ErrorCode}", request.Email,
                registerResult.Error.Code);
            return HandleFailure(registerResult);
        }

        _logger.LogInformation("User with email {Email} registered successfully.", request.Email);
        return NoContent();
    }
}