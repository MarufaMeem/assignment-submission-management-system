namespace AssignmentSystem.Api.DTOs.Users;

/// <summary>
/// Deliberately excludes Role and Email: changing a user's role after they've
/// already created assignments/submissions under their original role would
/// leave inconsistent history (e.g. a "former student" who is now a Teacher but
/// still has Submission rows as StudentId). Email is excluded because it's the
/// login identifier and changing it silently invites account-takeover confusion.
/// Assumption documented in README: to change role or email, deactivate the
/// account and create a new one.
/// </summary>
public class UpdateUserDto
{
    public string FullName { get; set; } = string.Empty;
    public int? ClassId { get; set; }
    public bool IsActive { get; set; }
}
