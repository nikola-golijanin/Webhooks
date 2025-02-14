namespace Webhooks.Domain.Models;

public sealed record User
{

    public string Email { get; init; }
    public Guid Id { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public DateTime CreatedOnUtc { get; init; }
    public ICollection<Role> Roles { get; set; }
}