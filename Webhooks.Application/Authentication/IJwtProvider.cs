using Webhooks.Domain.Models;

namespace Webhooks.Application.Authentication;

public interface IJwtProvider
{
    Task<string> GenerateTokenAsync(User user);
}