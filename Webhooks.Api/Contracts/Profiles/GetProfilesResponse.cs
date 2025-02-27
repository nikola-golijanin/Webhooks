using Webhooks.Domain.Models;

namespace Webhooks.Api.Contracts.Profiles;

public record GetProfilesResponse(int Id, string Name)
{
    private static GetProfilesResponse FromProfile(Profile profile) => new(profile.Id, profile.Name);

    public static IEnumerable<GetProfilesResponse> FromProfiles(IEnumerable<Profile> profiles) =>
        profiles.Select(FromProfile);
}