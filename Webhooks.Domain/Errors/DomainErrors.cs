using Webhooks.Domain.Models;
using Webhooks.Domain.Shared;

namespace Webhooks.Domain.Errors;

public static class DomainErrors
{
    public static class User
    {
        public static readonly Error InvalidCredentials = new(
           "User.InvalidCredentials",
           "The provided credentials are invalid");
    }
}
