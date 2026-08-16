using AssignmentSystem.Api.DTOs.TeacherAssignments;

namespace AssignmentSystem.Api.Services.Interfaces;

public interface ITeacherAssignmentService
{
    Task<List<TeacherAssignmentDto>> GetAllAsync(int? teacherId, CancellationToken ct = default);
    Task<TeacherAssignmentDto> CreateAsync(CreateTeacherAssignmentDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
