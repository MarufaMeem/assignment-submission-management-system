using AssignmentSystem.Api.Common;
using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.DTOs.Classes;
using AssignmentSystem.Api.Entities;
using AssignmentSystem.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Api.Services.Implementations;

public class ClassService : IClassService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<ClassService> _logger;

    public ClassService(ApplicationDbContext db, ILogger<ClassService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<ClassDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.Classes
            .OrderBy(c => c.Name)
            .Select(c => new ClassDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                StudentCount = c.Students.Count
            })
            .ToListAsync(ct);
    }

    public async Task<ClassDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var dto = await _db.Classes
            .Where(c => c.Id == id)
            .Select(c => new ClassDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                StudentCount = c.Students.Count
            })
            .FirstOrDefaultAsync(ct);

        return dto ?? throw new NotFoundAppException($"Class with id {id} was not found.");
    }

    public async Task<ClassDto> CreateAsync(UpsertClassDto dto, CancellationToken ct = default)
    {
        var entity = new Class { Name = dto.Name.Trim(), Description = dto.Description?.Trim() };
        _db.Classes.Add(entity);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Class {ClassId} ({Name}) created.", entity.Id, entity.Name);
        return await GetByIdAsync(entity.Id, ct);
    }

    public async Task<ClassDto> UpdateAsync(int id, UpsertClassDto dto, CancellationToken ct = default)
    {
        var entity = await _db.Classes.FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new NotFoundAppException($"Class with id {id} was not found.");

        entity.Name = dto.Name.Trim();
        entity.Description = dto.Description?.Trim();
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Class {ClassId} updated.", entity.Id);
        return await GetByIdAsync(entity.Id, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.Classes.FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new NotFoundAppException($"Class with id {id} was not found.");

        // Deletion is blocked (not cascaded) if the class is still in active use -
        // matches the FK Restrict behavior in ApplicationDbContext, but checking
        // here first gives a clear 409 with an explanatory message instead of
        // surfacing a raw database constraint-violation error to the client.
        var hasStudents = await _db.Users.AnyAsync(u => u.ClassId == id, ct);
        var hasAssignments = await _db.Assignments.IgnoreQueryFilters().AnyAsync(a => a.ClassId == id, ct);
        var hasGrants = await _db.TeacherClassSubjects.AnyAsync(t => t.ClassId == id, ct);

        if (hasStudents || hasAssignments || hasGrants)
        {
            throw new ConflictAppException(
                "This class cannot be deleted because it has enrolled students, assignments, " +
                "or teacher assignments referencing it. Reassign or remove those first.");
        }

        _db.Classes.Remove(entity);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Class {ClassId} deleted.", id);
    }
}
