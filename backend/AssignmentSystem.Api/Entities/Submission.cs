namespace AssignmentSystem.Api.Entities;

public class Submission
{
    public int Id { get; set; }

    public int AssignmentId { get; set; }
    public Assignment Assignment { get; set; } = null!;

    public int StudentId { get; set; }
    public User Student { get; set; } = null!;

    public string AnswerText { get; set; } = string.Empty;

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public SubmissionStatus Status { get; set; } = SubmissionStatus.Submitted;

    /// <summary>Null until reviewed. Must satisfy 0 <= Marks <= Assignment.MaxMarks.</summary>
    public decimal? Marks { get; set; }

    public string? Feedback { get; set; }

    public int? ReviewedByTeacherId { get; set; }
    public User? ReviewedByTeacher { get; set; }

    public DateTime? ReviewedAt { get; set; }
}
