namespace Webhooks.Infrastructure.QueryProjections;


public record UserIdWithProfileIds(int UserId, IEnumerable<int> ProfileIds);