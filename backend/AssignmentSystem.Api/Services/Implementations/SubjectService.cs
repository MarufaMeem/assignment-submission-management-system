using AssignmentSystem.Api.Common;
using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.DTOs.Subjects;
using AssignmentSystem.Api.Entities;
using AssignmentSystem.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Api.Services.Implementations;

public class SubjectService : ISubjectService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<SubjectService> _logger;

    public SubjectService(ApplicationDbContext db, ILogger<SubjectService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<SubjectDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.Subjects
            .OrderBy(s => s.Name)
            .Select(s => new SubjectDto { Id = s.Id, Name = s.Name, Code = s.Code })
            .ToListAsync(ct);
    }

    public async Task<SubjectDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var dto = await _db.Subjects
            .Where(s => s.Id == id)
            .Select(s => new SubjectDto { Id = s.Id, Name = s.Name, Code = s.Code })
            .FirstOrDefaultAsync(ct);

        return dto ?? throw new NotFoundAppException($"Subject with id {id} was not found.");
    }

    public async Task<SubjectDto> CreateAsync(UpsertSubjectDto dto, CancellationToken ct = default)
    {
        await EnsureCodeIsUniqueAsync(dto.Code, excludingId: null, ct);

        var entity = new Subject { Name = dto.Name.Trim(), Code = NormalizeCode(dto.Code) };
        _db.Subjects.Add(entity);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Subject {SubjectId} ({Name}) created.", entity.Id, entity.Name);
        return await GetByIdAsync(entity.Id, ct);
    }

    public async Task<SubjectDto> UpdateAsync(int id, UpsertSubjectDto dto, CancellationToken ct = default)
    {
        var entity = await _db.Subjects.FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new NotFoundAppException($"Subject with id {id} was not found.");

        await EnsureCodeIsUniqueAsync(dto.Code, excludingId: id, ct);

        entity.Name = dto.Name.Trim();
        entity.Code = NormalizeCode(dto.Code);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Subject {SubjectId} updated.", entity.Id);
        return await GetByIdAsync(entity.Id, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.Subjects.FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new NotFoundAppException($"Subject with id {id} was not found.");

        var hasAssignments = await _db.Assignments.IgnoreQueryFilters().AnyAsync(a => a.SubjectId == id, ct);
        var hasGrants = await _db.TeacherClassSubjects.AnyAsync(t => t.SubjectId == id, ct);

        if (hasAssignments || hasGrants)
        {
            throw new ConflictAppException(
                "This subject cannot be deleted because it has assignments or teacher " +
                "assignments referencing it. Reassign or remove those first.");
        }

        _db.Subjects.Remove(entity);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Subject {SubjectId} deleted.", id);
    }

    private static string? NormalizeCode(string? code) =>
        string.IsNullOrWhiteSpace(code) ? null : code.Trim().ToUpperInvariant();

    private async Task EnsureCodeIsUniqueAsync(string? code, int? excludingId, CancellationToken ct)
    {
        var normalized = NormalizeCode(code);
        if (normalized is null) return;

        var exists = await _db.Subjects.AnyAsync(
            s => s.Code != null && s.Code.ToUpper() == normalized && s.Id != excludingId, ct);

        if (exists)
        {
            throw new ConflictAppException($"A subject with code '{normalized}' already exists.");
        }
    }
}
