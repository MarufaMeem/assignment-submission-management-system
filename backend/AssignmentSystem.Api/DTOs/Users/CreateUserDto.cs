namespace AssignmentSystem.Api.DTOs.Users;

public class CreateUserDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // "Admin" | "Teacher" | "Student"

    /// <summary>Required and validated when Role == "Student"; must be omitted/null otherwise.</summary>
    public int? ClassId { get; set; }
}
