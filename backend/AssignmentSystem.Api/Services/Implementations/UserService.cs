using AssignmentSystem.Api.Common;
using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.DTOs.Users;
using AssignmentSystem.Api.Entities;
using AssignmentSystem.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Api.Services.Implementations;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _db;
    private readonly IPasswordHasherService _passwordHasher;
    private readonly ILogger<UserService> _logger;

    public UserService(ApplicationDbContext db, IPasswordHasherService passwordHasher, ILogger<UserService> logger)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<List<UserDto>> GetAllAsync(string? roleFilter, CancellationToken ct = default)
    {
        var query = _db.Users.Include(u => u.Class).AsQueryable();

        if (!string.IsNullOrWhiteSpace(roleFilter))
        {
            if (!Enum.TryParse<UserRole>(roleFilter, ignoreCase: true, out var role))
            {
                throw new ValidationAppException($"'{roleFilter}' is not a valid role.");
            }
            query = query.Where(u => u.Role == role);
        }

        var users = await query.OrderBy(u => u.FullName).ToListAsync(ct);
        return users.Select(MapToDto).ToList();
    }

    public async Task<UserDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var user = await _db.Users.Include(u => u.Class).FirstOrDefaultAsync(u => u.Id == id, ct)
            ?? throw new NotFoundAppException($"User with id {id} was not found.");

        return MapToDto(user);
    }

    public async Task<UserDto> CreateAsync(CreateUserDto dto, CancellationToken ct = default)
    {
        if (!Enum.TryParse<UserRole>(dto.Role, ignoreCase: true, out var role))
        {
            throw new ValidationAppException($"'{dto.Role}' is not a valid role. Must be Admin, Teacher, or Student.");
        }

        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        var emailExists = await _db.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail, ct);
        if (emailExists)
        {
            throw new ConflictAppException($"A user with email '{dto.Email}' already exists.");
        }

        // Business rule: ClassId is meaningful ONLY for Student accounts.
        if (role == UserRole.Student)
        {
            if (dto.ClassId is null)
            {
                throw new ValidationAppException("ClassId is required for a Student account.");
            }
            var classExists = await _db.Classes.AnyAsync(c => c.Id == dto.ClassId, ct);
            if (!classExists)
            {
                throw new ValidationAppException($"Class with id {dto.ClassId} was not found.");
            }
        }
        else if (dto.ClassId is not null)
        {
            throw new ValidationAppException("ClassId must not be set for Admin or Teacher accounts.");
        }

        var user = new User
        {
            FullName = dto.FullName.Trim(),
            Email = dto.Email.Trim(),
            PasswordHash = _passwordHasher.HashPassword(dto.Password),
            Role = role,
            ClassId = role == UserRole.Student ? dto.ClassId : null,
            IsActive = true
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("User {UserId} ({Role}) created by admin.", user.Id, user.Role);

        return await GetByIdAsync(user.Id, ct);
    }

    public async Task<UserDto> UpdateAsync(int id, UpdateUserDto dto, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct)
            ?? throw new NotFoundAppException($"User with id {id} was not found.");

        if (user.Role == UserRole.Student)
        {
            if (dto.ClassId is not null)
            {
                var classExists = await _db.Classes.AnyAsync(c => c.Id == dto.ClassId, ct);
                if (!classExists)
                {
                    throw new ValidationAppException($"Class with id {dto.ClassId} was not found.");
                }
            }
            user.ClassId = dto.ClassId;
        }
        else if (dto.ClassId is not null)
        {
            throw new ValidationAppException("ClassId must not be set for Admin or Teacher accounts.");
        }

        user.FullName = dto.FullName.Trim();
        user.IsActive = dto.IsActive;

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("User {UserId} updated by admin.", user.Id);

        return await GetByIdAsync(user.Id, ct);
    }

    public async Task DeactivateAsync(int id, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct)
            ?? throw new NotFoundAppException($"User with id {id} was not found.");

        // Soft-deactivate only (assumption A11) - never a hard delete, to preserve
        // referential history (assignments created, submissions made/reviewed).
        user.IsActive = false;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("User {UserId} deactivated by admin.", user.Id);
    }

    private static UserDto MapToDto(User user) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        Email = user.Email,
        Role = user.Role.ToString(),
        IsActive = user.IsActive,
        ClassId = user.ClassId,
        ClassName = user.Class?.Name,
        CreatedAt = user.CreatedAt
    };
}
