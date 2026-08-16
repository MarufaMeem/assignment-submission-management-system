using AssignmentSystem.Api.Common;
using AssignmentSystem.Api.DTOs.Users;
using AssignmentSystem.Api.Entities;
using AssignmentSystem.Api.Services.Implementations;
using AssignmentSystem.Tests.TestHelpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace AssignmentSystem.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<AssignmentSystem.Api.Services.Interfaces.IPasswordHasherService> _hasherMock = new();

    private UserService CreateSut(AssignmentSystem.Api.Data.ApplicationDbContext db)
    {
        _hasherMock.Setup(h => h.HashPassword(It.IsAny<string>())).Returns("hashed-value");
        return new UserService(db, _hasherMock.Object, NullLogger<UserService>.Instance);
    }

    // Business rule protected: emails must be unique (backend enforced, not just
    // a DB constraint that surfaces an ugly 500) - "Admin can manage users"
    // implicitly requires that duplicate creation attempts fail cleanly.
    [Fact]
    public async Task CreateAsync_DuplicateEmail_ThrowsConflict()
    {
        using var db = TestDbContextFactory.Create();
        db.Users.Add(new User { FullName = "Existing", Email = "dup@school.edu", PasswordHash = "x", Role = UserRole.Teacher, IsActive = true });
        await db.SaveChangesAsync();

        var sut = CreateSut(db);
        var dto = new CreateUserDto { FullName = "New Guy", Email = "dup@school.edu", Password = "Password1", Role = "Teacher" };

        await Assert.ThrowsAsync<ConflictAppException>(() => sut.CreateAsync(dto));
    }

    // Business rule protected: "a student must belong to a class" - ClassId is
    // required at creation time for Student accounts, not optional.
    [Fact]
    public async Task CreateAsync_StudentWithoutClassId_ThrowsValidation()
    {
        using var db = TestDbContextFactory.Create();
        var sut = CreateSut(db);
        var dto = new CreateUserDto { FullName = "New Student", Email = "s1@school.edu", Password = "Password1", Role = "Student", ClassId = null };

        await Assert.ThrowsAsync<ValidationAppException>(() => sut.CreateAsync(dto));
    }

    // Business rule protected: ClassId is only meaningful for students - an
    // Admin/Teacher account with a ClassId would be a data-integrity bug waiting
    // to confuse every downstream "does this user belong to this class" check.
    [Fact]
    public async Task CreateAsync_TeacherWithClassId_ThrowsValidation()
    {
        using var db = TestDbContextFactory.Create();
        db.Classes.Add(new Class { Id = 1, Name = "Class A" });
        await db.SaveChangesAsync();

        var sut = CreateSut(db);
        var dto = new CreateUserDto { FullName = "Mistaken Teacher", Email = "t1@school.edu", Password = "Password1", Role = "Teacher", ClassId = 1 };

        await Assert.ThrowsAsync<ValidationAppException>(() => sut.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_ValidStudent_SucceedsAndPersists()
    {
        using var db = TestDbContextFactory.Create();
        db.Classes.Add(new Class { Id = 1, Name = "Class A" });
        await db.SaveChangesAsync();

        var sut = CreateSut(db);
        var dto = new CreateUserDto { FullName = "Good Student", Email = "s2@school.edu", Password = "Password1", Role = "Student", ClassId = 1 };

        var result = await sut.CreateAsync(dto);

        Assert.Equal("Student", result.Role);
        Assert.Equal(1, result.ClassId);
        Assert.True(result.IsActive);
    }

    // Business rule protected: deactivation is soft (IsActive = false), never a
    // hard delete - preserves the user's history as submissions/assignments creator.
    [Fact]
    public async Task DeactivateAsync_SetsIsActiveFalse_UserStillExists()
    {
        using var db = TestDbContextFactory.Create();
        var user = new User { Id = 5, FullName = "ToDeactivate", Email = "d@school.edu", PasswordHash = "x", Role = UserRole.Student, IsActive = true, ClassId = null };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var sut = CreateSut(db);
        await sut.DeactivateAsync(5);

        var reloaded = await db.Users.FindAsync(5);
        Assert.NotNull(reloaded);
        Assert.False(reloaded!.IsActive);
    }
}
