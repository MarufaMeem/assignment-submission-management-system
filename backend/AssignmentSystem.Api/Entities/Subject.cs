namespace AssignmentSystem.Api.Entities;

public class Subject
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>Optional short code (e.g. "CS301"). Unique when provided.</summary>
    public string? Code { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    public ICollection<TeacherClassSubject> TeacherGrants { get; set; } = new List<TeacherClassSubject>();
}
