using Microsoft.EntityFrameworkCore;
using Webhooks.Api.Contracts.Users;
using Webhooks.Api.Data;
using Webhooks.Api.Models;

namespace Webhooks.Api.Services.Identity;

public class UserService
{
    private readonly WebhooksDbContext _context;
    private readonly JwtProvider _jwtProvider;
    
    public UserService(WebhooksDbContext context, JwtProvider jwtProvider)
    {
        _context = context;
        _jwtProvider = jwtProvider;
    }

    public async Task<string> LoginAsync(LoginUserRequest request)
    {
        User? user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);
        
        if (user is null)
            return string.Empty; //return error
        
        var token = _jwtProvider.Generate(user);
        return token;

    }
}