namespace AssignmentSystem.Api.DTOs.Submissions;

public class SubmissionResponseDto
{
    public int Id { get; set; }
    public int AssignmentId { get; set; }
    public string AssignmentTitle { get; set; } = string.Empty;
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string AnswerText { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal? Marks { get; set; }
    public string? Feedback { get; set; }
    public int? ReviewedByTeacherId { get; set; }
    public DateTime? ReviewedAt { get; set; }
}
