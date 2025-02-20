using Microsoft.AspNetCore.Authorization;
using Webhooks.Domain.Enums;

namespace Webhooks.Infrastructure.Authentication;

public class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(params Permission[] permissions)
        : base()
    {
        var permissionNames = permissions.Select(p => p.ToString());
        Roles = string.Join(',', permissionNames);
    }
}