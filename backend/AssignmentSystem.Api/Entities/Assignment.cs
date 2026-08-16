namespace AssignmentSystem.Api.Entities;

public class Assignment
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int ClassId { get; set; }
    public Class Class { get; set; } = null!;

    public int SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;

    public int CreatedByTeacherId { get; set; }
    public User CreatedByTeacher { get; set; } = null!;

    /// <summary>Stored as timestamptz; always compared against server UTC time.</summary>
    public DateTime Deadline { get; set; }

    public decimal MaxMarks { get; set; }

    public AssignmentStatus Status { get; set; } = AssignmentStatus.Draft;

    /// <summary>
    /// Assumption A2: late submissions are rejected by default. This flag lets a
    /// teacher opt in to accepting late submissions for a specific assignment,
    /// without building a full grace-period/penalty system.
    /// </summary>
    public bool AllowLateSubmission { get; set; } = false;

    /// <summary>
    /// Assumption A4: assignments are soft-deleted so existing submissions/grades
    /// are preserved for audit history. Excluded from all normal listing queries
    /// via an EF Core global query filter (configured in ApplicationDbContext).
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
