using Microsoft.EntityFrameworkCore;
using Webhooks.Application.Authentication;
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

    public async Task<Result> RegisterAsync(string email, CancellationToken cancellationToken)
    {
        var emailAlreadyExists = await _context.Users
            .AnyAsync(u => u.Email == email, cancellationToken);

        if (emailAlreadyExists)
            return Result.Failure(DomainErrors.User.EmailAlreadyExists(email));

        var defaultProfile = await _context.Set<Profile>()
            .FirstOrDefaultAsync(p => p.Name == "Subscriber", cancellationToken);

        if (defaultProfile is null)
            return Result.Failure(DomainErrors.Profile.NoProfilesFound);


        var user = new User
        {
            Email = email,
            CreatedOnUtc = DateTime.UtcNow,
            Profiles = [defaultProfile]
        };

        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
        //TODO maybe after registration automatically login user
    }
}