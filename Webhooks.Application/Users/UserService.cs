using Microsoft.EntityFrameworkCore;
using Webhooks.Application.Abstractions;
using Webhooks.Domain.Models;
using Webhooks.Persistance;

namespace Webhooks.Application.Users;

public class UserService : IUserService
{
    private readonly WebhooksDbContext _context;
    private readonly IJwtProvider _jwtProvider;

    public UserService(WebhooksDbContext context, IJwtProvider jwtProvider)
    {
        _context = context;
        _jwtProvider = jwtProvider;
    }

    public async Task<string> LoginAsync(string email)
    {
        User? user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user is null)
            return string.Empty; //return error

        var token = await _jwtProvider.GenerateTokenAsync(user);
        return token;
    }
}