using System.ComponentModel.DataAnnotations.Schema;

namespace Webhooks.Domain.Models;

public sealed class User
{
    [Column("id")]
    public int Id { get; init; }

    [Column("email")]
    public string Email { get; init; } = string.Empty;

    [Column("first_name")]
    public string FirstName { get; init; } = string.Empty;

    [Column("last_name")]
    public string LastName { get; init; } = string.Empty;

    [Column("created_on_utc")]
    public DateTime CreatedOnUtc { get; init; }
    public ICollection<Profile> Profiles { get; set; } = [];
}