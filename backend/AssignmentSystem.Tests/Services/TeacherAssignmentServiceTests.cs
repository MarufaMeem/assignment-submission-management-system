using AssignmentSystem.Api.Common;
using AssignmentSystem.Api.DTOs.TeacherAssignments;
using AssignmentSystem.Api.Entities;
using AssignmentSystem.Api.Services.Implementations;
using AssignmentSystem.Tests.TestHelpers;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AssignmentSystem.Tests.Services;

public class TeacherAssignmentServiceTests
{
    private static TeacherAssignmentService CreateSut(AssignmentSystem.Api.Data.ApplicationDbContext db) =>
        new(db, NullLogger<TeacherAssignmentService>.Instance);

    private static async Task<(Class cls, Subject subj, User teacher)> SeedBasicsAsync(AssignmentSystem.Api.Data.ApplicationDbContext db)
    {
        var cls = new Class { Name = "Class A" };
        var subj = new Subject { Name = "Math" };
        var teacher = new User { FullName = "Teacher One", Email = "t1@school.edu", PasswordHash = "x", Role = UserRole.Teacher, IsActive = true };
        db.AddRange(cls, subj, teacher);
        await db.SaveChangesAsync();
        return (cls, subj, teacher);
    }

    // Business rule protected: a grant is a unique (teacher, class, subject)
    // triple - duplicates would make "is this teacher authorized" ambiguous
    // and are explicitly disallowed by the schema's unique index (Phase 1).
    [Fact]
    public async Task CreateAsync_DuplicateGrant_ThrowsConflict()
    {
        using var db = TestDbContextFactory.Create();
        var (cls, subj, teacher) = await SeedBasicsAsync(db);
        db.TeacherClassSubjects.Add(new TeacherClassSubject { TeacherId = teacher.Id, ClassId = cls.Id, SubjectId = subj.Id });
        await db.SaveChangesAsync();

        var sut = CreateSut(db);
        var dto = new CreateTeacherAssignmentDto { TeacherId = teacher.Id, ClassId = cls.Id, SubjectId = subj.Id };

        await Assert.ThrowsAsync<ConflictAppException>(() => sut.CreateAsync(dto));
    }

    // Business rule protected: only users with Role == Teacher can receive a
    // teaching grant - granting one to a Student or Admin account would be a
    // silent authorization-model bug (they'd never be checked as owners, but
    // the row would sit there as confusing dead data).
    [Fact]
    public async Task CreateAsync_NonTeacherUser_ThrowsValidation()
    {
        using var db = TestDbContextFactory.Create();
        var cls = new Class { Name = "Class A" };
        var subj = new Subject { Name = "Math" };
        var student = new User { FullName = "A Student", Email = "s1@school.edu", PasswordHash = "x", Role = UserRole.Student, IsActive = true };
        db.AddRange(cls, subj, student);
        await db.SaveChangesAsync();

        var sut = CreateSut(db);
        var dto = new CreateTeacherAssignmentDto { TeacherId = student.Id, ClassId = cls.Id, SubjectId = subj.Id };

        await Assert.ThrowsAsync<ValidationAppException>(() => sut.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_ValidGrant_Succeeds()
    {
        using var db = TestDbContextFactory.Create();
        var (cls, subj, teacher) = await SeedBasicsAsync(db);

        var sut = CreateSut(db);
        var dto = new CreateTeacherAssignmentDto { TeacherId = teacher.Id, ClassId = cls.Id, SubjectId = subj.Id };

        var result = await sut.CreateAsync(dto);

        Assert.Equal(teacher.Id, result.TeacherId);
        Assert.Equal(cls.Id, result.ClassId);
        Assert.Equal(subj.Id, result.SubjectId);
    }
}
