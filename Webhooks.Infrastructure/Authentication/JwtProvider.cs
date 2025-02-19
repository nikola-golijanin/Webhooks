using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Webhooks.Application.Abstractions;
using Webhooks.Domain.Models;

namespace Webhooks.Infrastructure.Authentication;

public sealed class JwtProvider : IJwtProvider
{
    private readonly JwtOptions _jwtOptions;
    private readonly PermissionService _permissionService;

    public JwtProvider(IOptions<JwtOptions> jwtOptions, PermissionService permissionService)
    {
        _permissionService = permissionService;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<string> GenerateTokenAsync(User user)
    {

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
        };

        var permissions = await _permissionService.GetPermissionsAsync(user.Id);
        claims.AddRange(permissions.Select(permission => new Claim(ClaimTypes.Role, permission)));

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            _jwtOptions.Issuer,
            _jwtOptions.Audience,
            claims,
            null,
            DateTime.UtcNow.AddHours(1),
            signingCredentials);

        var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);
        return tokenValue;
    }
}