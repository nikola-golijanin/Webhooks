using Microsoft.AspNetCore.Mvc;
using Webhooks.Api.Contracts.Users;
using Webhooks.Application.Users;

namespace Webhooks.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    public async Task<IActionResult> LoginUserAsync([FromBody] LoginUserRequest request, CancellationToken cancellationToken)
    {
        var token = await _userService.LoginAsync(request.Email);

        if (string.Empty.Equals(token))
            return BadRequest();

        return Ok(token);
    }
}