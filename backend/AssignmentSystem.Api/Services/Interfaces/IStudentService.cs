using AssignmentSystem.Api.DTOs.Students;
using AssignmentSystem.Api.DTOs.Submissions;

namespace AssignmentSystem.Api.Services.Interfaces;

public interface IStudentService
{
    Task<List<StudentAssignmentResponseDto>> GetAvailableAssignmentsAsync(int studentId, CancellationToken ct);
    Task<StudentAssignmentResponseDto> GetAssignmentByIdAsync(int studentId, int assignmentId, CancellationToken ct);
    Task<SubmissionResponseDto> CreateSubmissionAsync(int studentId, int assignmentId, CreateSubmissionDto dto, CancellationToken ct);
    Task<SubmissionResponseDto> UpdateSubmissionAsync(int studentId, int assignmentId, int submissionId, UpdateSubmissionDto dto, CancellationToken ct);
    Task<SubmissionResponseDto> GetMySubmissionAsync(int studentId, int assignmentId, CancellationToken ct);
}
