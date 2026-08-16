using AssignmentSystem.Api.Common;
using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.DTOs.TeacherAssignments;
using AssignmentSystem.Api.Entities;
using AssignmentSystem.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Api.Services.Implementations;

public class TeacherAssignmentService : ITeacherAssignmentService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<TeacherAssignmentService> _logger;

    public TeacherAssignmentService(ApplicationDbContext db, ILogger<TeacherAssignmentService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<TeacherAssignmentDto>> GetAllAsync(int? teacherId, CancellationToken ct = default)
    {
        var query = _db.TeacherClassSubjects
            .Include(t => t.Teacher)
            .Include(t => t.Class)
            .Include(t => t.Subject)
            .AsQueryable();

        if (teacherId is not null)
        {
            query = query.Where(t => t.TeacherId == teacherId);
        }

        var grants = await query.OrderBy(t => t.Teacher.FullName).ToListAsync(ct);
        return grants.Select(MapToDto).ToList();
    }

    public async Task<TeacherAssignmentDto> CreateAsync(CreateTeacherAssignmentDto dto, CancellationToken ct = default)
    {
        var teacher = await _db.Users.FirstOrDefaultAsync(u => u.Id == dto.TeacherId, ct)
            ?? throw new ValidationAppException($"User with id {dto.TeacherId} was not found.");

        if (teacher.Role != UserRole.Teacher)
        {
            throw new ValidationAppException($"User {teacher.FullName} is not a Teacher and cannot be granted a class/subject assignment.");
        }

        var classExists = await _db.Classes.AnyAsync(c => c.Id == dto.ClassId, ct);
        if (!classExists) throw new ValidationAppException($"Class with id {dto.ClassId} was not found.");

        var subjectExists = await _db.Subjects.AnyAsync(s => s.Id == dto.SubjectId, ct);
        if (!subjectExists) throw new ValidationAppException($"Subject with id {dto.SubjectId} was not found.");

        // Pre-check rather than relying solely on catching the DB unique-constraint
        // violation - gives a clean 409 with a clear message instead of leaking a
        // provider-specific exception through to the client.
        var duplicateExists = await _db.TeacherClassSubjects.AnyAsync(
            t => t.TeacherId == dto.TeacherId && t.ClassId == dto.ClassId && t.SubjectId == dto.SubjectId, ct);

        if (duplicateExists)
        {
            throw new ConflictAppException("This teacher already has this exact class/subject assignment.");
        }

        var grant = new TeacherClassSubject
        {
            TeacherId = dto.TeacherId,
            ClassId = dto.ClassId,
            SubjectId = dto.SubjectId
        };

        _db.TeacherClassSubjects.Add(grant);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Teacher {TeacherId} granted Class {ClassId} / Subject {SubjectId}.",
            dto.TeacherId, dto.ClassId, dto.SubjectId);

        var created = await _db.TeacherClassSubjects
            .Include(t => t.Teacher).Include(t => t.Class).Include(t => t.Subject)
            .FirstAsync(t => t.Id == grant.Id, ct);

        return MapToDto(created);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var grant = await _db.TeacherClassSubjects.FirstOrDefaultAsync(t => t.Id == id, ct)
            ?? throw new NotFoundAppException($"Teacher assignment with id {id} was not found.");

        // Revoking a grant does NOT retroactively affect assignments the teacher
        // already created under it (see Phase 1 schema rationale - Assignment's
        // FK to the teacher is independent of TeacherClassSubject's continued
        // existence, so assignment history/grading is preserved).
        _db.TeacherClassSubjects.Remove(grant);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Teacher assignment {GrantId} revoked.", id);
    }

    private static TeacherAssignmentDto MapToDto(TeacherClassSubject t) => new()
    {
        Id = t.Id,
        TeacherId = t.TeacherId,
        TeacherName = t.Teacher.FullName,
        ClassId = t.ClassId,
        ClassName = t.Class.Name,
        SubjectId = t.SubjectId,
        SubjectName = t.Subject.Name,
        AssignedAt = t.AssignedAt
    };
}
