using System.Collections;

namespace Webhooks.Domain.Models;

public record Role
{
    public static readonly Role Registered = new(1, "Registered");
    public Role(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public int Id { get; set; }

    public string Name { get; set; }

    public ICollection<Permission> Permissions { get; set; }

    public ICollection<User> Users { get; set; }
};