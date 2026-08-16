using System.ComponentModel.DataAnnotations;

namespace AssignmentSystem.Api.DTOs.Students;

public class UpdateSubmissionDto
{
    [Required]
    [MinLength(1, ErrorMessage = "Answer cannot be empty.")]
    public string AnswerText { get; set; } = string.Empty;
}
