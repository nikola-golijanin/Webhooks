using System.ComponentModel.DataAnnotations.Schema;

namespace Webhooks.Domain.Models;

public class Permission
{
    [Column("id")]
    public int Id { get; init; }

    [Column("name")]
    public string Name { get; init; } = string.Empty;
};