using Webhooks.Domain.Shared;

namespace Webhooks.Application.Users;

public interface IUserService
{
    Task<Result<string>> LoginAsync(string email, CancellationToken cancellationToken);
    Task<Result> RegisterAsync(string email,  CancellationToken cancellationToken);
}