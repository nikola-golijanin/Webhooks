using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;

namespace Webhooks.Api.Authentication;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override  Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var permissions = context.User.Claims.Where(c => c.Type == "permissions")
            .Select(c => c.Value)
            .ToHashSet();
       
        if(permissions.Contains(requirement.Permission))
            context.Succeed(requirement);
        
        return Task.CompletedTask;
    }
}