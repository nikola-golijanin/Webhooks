using Microsoft.AspNetCore.Mvc;
using Webhooks.Api.Contracts.Users;
using Webhooks.Api.Services.Identity;

namespace Webhooks.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;

    public UsersController(UserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    public async Task<IActionResult> LoginUserAsync([FromBody] LoginUserRequest request,CancellationToken cancellationToken)
    {
        var token = await _userService.LoginAsync(request);
        
        if (string.Empty.Equals(token))
            return BadRequest();
        
        return Ok(token);
    }    
}