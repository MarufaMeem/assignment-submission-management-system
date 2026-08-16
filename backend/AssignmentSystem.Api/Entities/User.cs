namespace AssignmentSystem.Api.Entities;

/// <summary>
/// Represents an Admin, Teacher, or Student. A single table is used for all
/// three roles (discriminated by <see cref="Role"/>) rather than three separate
/// tables, because they share the same core identity/auth fields and there is
/// no role-specific data heavy enough to justify table-per-type inheritance.
///
/// ClassId is only meaningful when Role == Student (assumption A6: a student
/// belongs to exactly one class at a time). It is null for Admin and Teacher.
/// </summary>
public class User
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    /// <summary>Login identifier. Unique constraint enforced in ApplicationDbContext.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Never plaintext. Produced by ASP.NET Core's PasswordHasher&lt;User&gt;.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    /// <summary>
    /// Soft-deactivation flag (assumption A11). A deactivated user cannot log in,
    /// but their historical data (assignments created, submissions made) is preserved.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Only set when Role == Student. FK to Class.</summary>
    public int? ClassId { get; set; }
    public Class? Class { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation: assignments this user created (only meaningful for Teacher role)
    public ICollection<Assignment> AssignmentsCreated { get; set; } = new List<Assignment>();

    // Navigation: submissions this user made (only meaningful for Student role)
    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();

    // Navigation: teacher-class-subject grants held by this user (only meaningful for Teacher role)
    public ICollection<TeacherClassSubject> TeachingGrants { get; set; } = new List<TeacherClassSubject>();
}
