using AssignmentSystem.Api.Entities;

namespace AssignmentSystem.Api.DTOs.Assignments;

public class AssignmentResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public int SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public int CreatedByTeacherId { get; set; }
    public string CreatedByTeacherName { get; set; } = string.Empty;
    public DateTime Deadline { get; set; }
    public decimal MaxMarks { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool AllowLateSubmission { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
