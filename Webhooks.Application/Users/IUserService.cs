namespace Webhooks.Application.Users;

public interface IUserService
{
    Task<string> LoginAsync(string email);
}
