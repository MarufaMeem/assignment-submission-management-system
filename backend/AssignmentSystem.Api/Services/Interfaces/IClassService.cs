using AssignmentSystem.Api.DTOs.Classes;

namespace AssignmentSystem.Api.Services.Interfaces;

public interface IClassService
{
    Task<List<ClassDto>> GetAllAsync(CancellationToken ct = default);
    Task<ClassDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ClassDto> CreateAsync(UpsertClassDto dto, CancellationToken ct = default);
    Task<ClassDto> UpdateAsync(int id, UpsertClassDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
