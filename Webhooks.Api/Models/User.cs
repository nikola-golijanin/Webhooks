namespace Webhooks.Api.Models;

public sealed record User(Guid Id, string Email, string FirstName, string LastName,DateTime CreatedOnUtc);