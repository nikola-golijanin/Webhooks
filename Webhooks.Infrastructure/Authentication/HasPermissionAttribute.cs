using Microsoft.AspNetCore.Authorization;
using Webhooks.Domain.Enums;

namespace Webhooks.Infrastructure.Authentication;

public class HasPermissionAttribute : AuthorizeAttribute
{
    private static readonly AuthScheme[] DefaultAuthSchemes = [AuthScheme.WebhooksApi];

    public HasPermissionAttribute(params Permission[] Permissions)
        : this(DefaultAuthSchemes, Permissions)
    {
    }

    public HasPermissionAttribute(AuthScheme[] AuthSchemes, params Permission[] Permissions)
        : base()
    {
        AuthenticationSchemes = string.Join(',', AuthSchemes.Select(s => s.ToString()));
        if (Permissions.Length > 0)
            Roles = string.Join(',', Permissions.Select(p => p.ToString()));
    }
}