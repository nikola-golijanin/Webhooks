using Microsoft.AspNetCore.Authorization;
using Webhooks.Domain.Enums;

namespace Webhooks.Infrastructure.Authentication;

public class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(Permission permission)
        : base(policy: permission.ToString())
    {
    }
}
