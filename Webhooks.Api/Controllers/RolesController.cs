using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Webhooks.Api.Contracts.Roles;
using Webhooks.Domain.Enums;
using Webhooks.Infrastructure.Authentication;
using Webhooks.Persistance;

namespace Webhooks.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [HasPermission(Permission.AccessRoles)]
    public class RolesController : ControllerBase
    {
        private readonly WebhooksDbContext _context;

        public RolesController(WebhooksDbContext context)
        {
            _context = context;
        }

        //Generate endpoint that will list all roles
        [HttpGet]
        [HasPermission(Permission.ReadRoles)]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _context.Roles.Select(r => new GetRolesResponse(r.Id, r.Name)).ToListAsync();
            return Ok(roles);
        }
    }
}
