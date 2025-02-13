using Microsoft.AspNetCore.Authorization;

namespace Webhooks.Api.Authentication;

public class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(Permission permission) : base(policy: permission.ToString())
    {
    }
}