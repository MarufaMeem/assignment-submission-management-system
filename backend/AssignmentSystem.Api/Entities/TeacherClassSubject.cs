namespace AssignmentSystem.Api.Entities;

/// <summary>
/// Grants a Teacher permission to operate on a specific (Class, Subject) pair.
/// This is the row every assignment-creation/update authorization check is
/// validated against - see business rule "a teacher should only manage
/// assignments they are authorized to manage".
///
/// Unique constraint on (TeacherId, ClassId, SubjectId) enforced in
/// ApplicationDbContext - prevents duplicate grants and makes the
/// authorization check a single EXISTS query.
/// </summary>
public class TeacherClassSubject
{
    public int Id { get; set; }

    public int TeacherId { get; set; }
    public User Teacher { get; set; } = null!;

    public int ClassId { get; set; }
    public Class Class { get; set; } = null!;

    public int SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}
