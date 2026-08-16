using AssignmentSystem.Api.DTOs.Subjects;

namespace AssignmentSystem.Api.Services.Interfaces;

public interface ISubjectService
{
    Task<List<SubjectDto>> GetAllAsync(CancellationToken ct = default);
    Task<SubjectDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<SubjectDto> CreateAsync(UpsertSubjectDto dto, CancellationToken ct = default);
    Task<SubjectDto> UpdateAsync(int id, UpsertSubjectDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
