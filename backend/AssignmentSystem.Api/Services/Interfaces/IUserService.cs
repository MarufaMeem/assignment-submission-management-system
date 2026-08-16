using AssignmentSystem.Api.DTOs.Users;

namespace AssignmentSystem.Api.Services.Interfaces;

public interface IUserService
{
    Task<List<UserDto>> GetAllAsync(string? roleFilter, CancellationToken ct = default);
    Task<UserDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<UserDto> CreateAsync(CreateUserDto dto, CancellationToken ct = default);
    Task<UserDto> UpdateAsync(int id, UpdateUserDto dto, CancellationToken ct = default);
    Task DeactivateAsync(int id, CancellationToken ct = default);
}
