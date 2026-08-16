namespace AssignmentSystem.Api.DTOs.Users;

public class UserDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int? ClassId { get; set; }
    public string? ClassName { get; set; }
    public DateTime CreatedAt { get; set; }
}
