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

    public static class Profile
    {
        public static readonly Error NoProfilesFound = new(
            "Profile.NoProfilesFound",
            "No profiles found");

        public static readonly Func<int, Error> NoProfilesForUserFound = userId => new Error(
            "Profile.NoProfilesForUserFound",
            $"No profiles found for user with id {userId}");

        public static readonly Func<int, Error> ProfileNotFound = profileId => new Error(
            "Profile.ProfileNotFound",
            $"Profile with id {profileId} not found");
    }
}