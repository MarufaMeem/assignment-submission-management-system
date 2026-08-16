using System.ComponentModel.DataAnnotations;

namespace AssignmentSystem.Api.DTOs.Submissions;

public class ReviewSubmissionDto
{
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Marks cannot be negative.")]
    public decimal Marks { get; set; }

    public string? Feedback { get; set; }
}
