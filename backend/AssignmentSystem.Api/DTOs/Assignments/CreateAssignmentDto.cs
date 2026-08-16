using System.ComponentModel.DataAnnotations;

namespace AssignmentSystem.Api.DTOs.Assignments;

public class CreateAssignmentDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public int ClassId { get; set; }

    [Required]
    public int SubjectId { get; set; }

    [Required]
    public DateTime Deadline { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Maximum marks must be greater than zero.")]
    public decimal MaxMarks { get; set; }

    public bool AllowLateSubmission { get; set; } = false;
}
