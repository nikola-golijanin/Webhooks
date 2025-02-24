using System.ComponentModel.DataAnnotations;

namespace Webhooks.Api.Contracts.Users;

public record RegisterUserRequest([EmailAddress] string Email);