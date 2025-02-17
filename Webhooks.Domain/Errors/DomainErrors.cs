using Webhooks.Domain.Shared;

namespace Webhooks.Domain.Errors;

public static class DomainErrors
{
    public static class User
    {
        public static readonly Error InvalidCredentials = new(
           "User.InvalidCredentials",
           "The provided credentials are invalid");

        public static readonly Func<int, Error> UserNotFound = userId => new Error(
                    "User.UserNotFound",
                    $"User with id {userId} not found");
    }

    public static class Role
    {
        public static readonly Error NoRolesFound = new(
            "Role.NoRolesFound",
            "No roles found");

        public static readonly Func<int, Error> NoRolesForUserFound = userId => new Error(
            "Role.NoRolesForUserFound",
            $"No roles found for user with id {userId}");

        public static readonly Func<int, Error> RoleNotFound = roleId => new Error(
            "Role.RoleNotFound",
            $"Role with id {roleId} not found");

    }
}
