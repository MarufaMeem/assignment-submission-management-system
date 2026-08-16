using AssignmentSystem.Api.Common;
using AssignmentSystem.Api.DTOs.Classes;
using AssignmentSystem.Api.Entities;
using AssignmentSystem.Api.Services.Implementations;
using AssignmentSystem.Tests.TestHelpers;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AssignmentSystem.Tests.Services;

public class ClassServiceTests
{
    private static ClassService CreateSut(AssignmentSystem.Api.Data.ApplicationDbContext db) =>
        new(db, NullLogger<ClassService>.Instance);

    // Business rule protected: a class with enrolled students cannot be deleted
    // outright - deleting it would orphan those students' ClassId reference.
    [Fact]
    public async Task DeleteAsync_ClassWithEnrolledStudents_ThrowsConflict()
    {
        using var db = TestDbContextFactory.Create();
        var cls = new Class { Id = 1, Name = "Class A" };
        db.Classes.Add(cls);
        db.Users.Add(new User { FullName = "Student", Email = "s@school.edu", PasswordHash = "x", Role = UserRole.Student, ClassId = 1, IsActive = true });
        await db.SaveChangesAsync();

        var sut = CreateSut(db);

        await Assert.ThrowsAsync<ConflictAppException>(() => sut.DeleteAsync(1));
    }

    [Fact]
    public async Task DeleteAsync_UnusedClass_Succeeds()
    {
        using var db = TestDbContextFactory.Create();
        db.Classes.Add(new Class { Id = 2, Name = "Empty Class" });
        await db.SaveChangesAsync();

        var sut = CreateSut(db);
        await sut.DeleteAsync(2);

        Assert.Null(await db.Classes.FindAsync(2));
    }

    [Fact]
    public async Task CreateAsync_ValidInput_ReturnsClassWithZeroStudentCount()
    {
        using var db = TestDbContextFactory.Create();
        var sut = CreateSut(db);

        var result = await sut.CreateAsync(new UpsertClassDto { Name = "New Class", Description = "desc" });

        Assert.Equal("New Class", result.Name);
        Assert.Equal(0, result.StudentCount);
    }
}
