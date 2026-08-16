namespace AssignmentSystem.Api.DTOs.Students;

public class StudentAssignmentResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string CreatedByTeacherName { get; set; } = string.Empty;
    public DateTime Deadline { get; set; }
    public decimal MaxMarks { get; set; }
    public bool AllowLateSubmission { get; set; }
}
