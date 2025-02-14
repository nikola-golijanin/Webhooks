using System;
using Webhooks.Domain.Models;

namespace Webhooks.Application.Abstractions;

public interface IJwtProvider
{
    Task<string> GenerateTokenAsync(User user);
}
