namespace Webhooks.Persistance.QueryProjections;

public record UserIdWithProfileIds(int UserId, IEnumerable<int> ProfileIds);