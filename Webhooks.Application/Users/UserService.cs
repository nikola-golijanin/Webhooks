using Webhooks.Application.Authentication;
using Webhooks.Domain.Errors;
using Webhooks.Domain.Models;
using Webhooks.Domain.Shared;
using Webhooks.Persistance;
using Webhooks.Persistance.Queries;

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
        var user = await _context.Users
            .FindByEmailAsync(email, cancellationToken);

        if (user is null)
            return Result.Failure<string>(DomainErrors.User.InvalidCredentials);

        var token = await _jwtProvider.GenerateTokenAsync(user);
        return token;
    }


    public async Task<Result> RegisterAsync(string email, CancellationToken cancellationToken)
    {
        var emailAlreadyExists = await _context.Users.IsEmailRegisteredAsync(email, cancellationToken);

        if (emailAlreadyExists)
            return Result.Failure(DomainErrors.User.EmailAlreadyExists(email));

        var defaultProfile = await _context.Set<Profile>()
            .GetDefaultProfileAsync(cancellationToken);

        ArgumentNullException.ThrowIfNull(defaultProfile, nameof(defaultProfile));

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