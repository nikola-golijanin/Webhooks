using System.ComponentModel.DataAnnotations.Schema;

namespace Webhooks.Domain.Models;

public sealed class Profile
{
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    public ICollection<Permission> Permissions { get; set; } = [];

    public ICollection<User> Users { get; set; } = [];
};