using AssignmentSystem.Api.DTOs.Assignments;
using AssignmentSystem.Api.DTOs.Submissions;

namespace AssignmentSystem.Api.Services.Interfaces;

public interface IAssignmentService
{
    Task<AssignmentResponseDto> CreateAsync(CreateAssignmentDto dto, int teacherId, CancellationToken ct);
    Task<AssignmentResponseDto> UpdateAsync(int id, UpdateAssignmentDto dto, int teacherId, CancellationToken ct);
    Task DeleteAsync(int id, int teacherId, CancellationToken ct);
    Task<AssignmentResponseDto> PublishAsync(int id, int teacherId, CancellationToken ct);
    Task<List<AssignmentResponseDto>> GetByTeacherAsync(int teacherId, CancellationToken ct);
    Task<AssignmentResponseDto> GetByIdAsync(int id, int teacherId, CancellationToken ct);
    Task<List<SubmissionResponseDto>> GetSubmissionsAsync(int assignmentId, int teacherId, CancellationToken ct);
    Task<SubmissionResponseDto> ReviewSubmissionAsync(int assignmentId, int submissionId, ReviewSubmissionDto dto, int teacherId, CancellationToken ct);
}
