using Microsoft.EntityFrameworkCore;
using Webhooks.Application.Abstractions;
using Webhooks.Domain.Errors;
using Webhooks.Domain.Models;
using Webhooks.Domain.Shared;
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

    public async Task<Result<string>> LoginAsync(string email, CancellationToken cancellationToken)
    {
        User? user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user is null)
            return Result.Failure<string>(DomainErrors.User.InvalidCredentials);

        var token = await _jwtProvider.GenerateTokenAsync(user);
        return token;
    }
}