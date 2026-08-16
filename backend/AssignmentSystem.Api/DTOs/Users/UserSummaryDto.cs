namespace AssignmentSystem.Api.DTOs.Users;

/// <summary>
/// Deliberately excludes PasswordHash and any other sensitive field - this is
/// what "DTOs instead of exposing database entities directly" means in practice:
/// even a field that's merely unnecessary (not just sensitive) is left out so a
/// future change to User never accidentally leaks through an old DTO shape.
/// </summary>
public class UserSummaryDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int? ClassId { get; set; }
}
