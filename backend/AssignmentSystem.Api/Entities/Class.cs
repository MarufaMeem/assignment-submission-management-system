namespace AssignmentSystem.Api.Entities;

/// <summary>
/// Represents a Class/Course (assumption A10: treated as the same entity -
/// e.g. "10th Grade - Section A" or "CS301 - Data Structures" both fit here).
/// </summary>
public class Class
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<User> Students { get; set; } = new List<User>();
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    public ICollection<TeacherClassSubject> TeacherGrants { get; set; } = new List<TeacherClassSubject>();
}
