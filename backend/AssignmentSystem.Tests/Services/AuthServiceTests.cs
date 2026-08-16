using AssignmentSystem.Api.Common;
using AssignmentSystem.Api.DTOs.Auth;
using AssignmentSystem.Api.Entities;
using AssignmentSystem.Api.Services.Implementations;
using AssignmentSystem.Api.Services.Interfaces;
using AssignmentSystem.Tests.TestHelpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace AssignmentSystem.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IPasswordHasherService> _hasherMock = new();
    private readonly Mock<IJwtTokenGenerator> _tokenGeneratorMock = new();

    private AuthService CreateSut(AssignmentSystem.Api.Data.ApplicationDbContext db) =>
        new(db, _hasherMock.Object, _tokenGeneratorMock.Object, NullLogger<AuthService>.Instance);

    // Business rule protected: authentication must succeed for a correctly
    // matched, active user, and must return a usable token + safe user summary
    // (no PasswordHash leaking through the DTO).
    [Fact]
    public async Task LoginAsync_ValidActiveUserCorrectPassword_ReturnsTokenAndUser()
    {
        using var db = TestDbContextFactory.Create();
        var user = new User
        {
            Id = 1,
            FullName = "Alice Johnson",
            Email = "alice.teacher@school.edu",
            PasswordHash = "irrelevant-hash",
            Role = UserRole.Teacher,
            IsActive = true
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        _hasherMock.Setup(h => h.VerifyPassword("irrelevant-hash", "Teacher@123")).Returns(true);
        _tokenGeneratorMock.Setup(t => t.GenerateToken(It.Is<User>(u => u.Id == 1)))
            .Returns(("fake-jwt-token", DateTime.UtcNow.AddHours(1)));

        var sut = CreateSut(db);

        var result = await sut.LoginAsync(new LoginRequestDto { Email = "alice.teacher@school.edu", Password = "Teacher@123" });

        Assert.Equal("fake-jwt-token", result.Token);
        Assert.Equal("Teacher", result.User.Role);
        Assert.Equal(user.Email, result.User.Email);
    }

    // Business rule protected: "invalid login" - a wrong password must be rejected,
    // never succeed, regardless of how close it is to the real password.
    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorized()
    {
        using var db = TestDbContextFactory.Create();
        db.Users.Add(new User
        {
            Id = 2, FullName = "Bob", Email = "bob@school.edu",
            PasswordHash = "some-hash", Role = UserRole.Teacher, IsActive = true
        });
        await db.SaveChangesAsync();

        _hasherMock.Setup(h => h.VerifyPassword("some-hash", It.IsAny<string>())).Returns(false);
        var sut = CreateSut(db);

        await Assert.ThrowsAsync<UnauthorizedAppException>(() =>
            sut.LoginAsync(new LoginRequestDto { Email = "bob@school.edu", Password = "WrongPassword" }));
    }

    // Business rule protected: "nonexistent user" login attempts must fail cleanly
    // (never throw an unhandled null-reference exception, never succeed).
    [Fact]
    public async Task LoginAsync_UnknownEmail_ThrowsUnauthorized()
    {
        using var db = TestDbContextFactory.Create();
        var sut = CreateSut(db);

        await Assert.ThrowsAsync<UnauthorizedAppException>(() =>
            sut.LoginAsync(new LoginRequestDto { Email = "nobody@school.edu", Password = "whatever" }));
    }

    // Business rule protected: "inactive user" behavior - a deactivated account
    // must not be able to log in even with the exactly correct password.
    [Fact]
    public async Task LoginAsync_InactiveUser_ThrowsUnauthorizedEvenWithCorrectPassword()
    {
        using var db = TestDbContextFactory.Create();
        db.Users.Add(new User
        {
            Id = 3, FullName = "Fiona", Email = "fiona@school.edu",
            PasswordHash = "correct-hash", Role = UserRole.Student, IsActive = false
        });
        await db.SaveChangesAsync();

        // Even if the hasher WOULD say this password is correct, IsActive is
        // checked and must short-circuit before password verification succeeds.
        _hasherMock.Setup(h => h.VerifyPassword("correct-hash", "Student@123")).Returns(true);
        var sut = CreateSut(db);

        await Assert.ThrowsAsync<UnauthorizedAppException>(() =>
            sut.LoginAsync(new LoginRequestDto { Email = "fiona@school.edu", Password = "Student@123" }));
    }

    // Security-critical rule: unknown-email and inactive-account failures must be
    // INDISTINGUISHABLE to the caller (same exception message), otherwise the
    // login endpoint becomes an oracle for enumerating valid/deactivated emails.
    [Fact]
    public async Task LoginAsync_UnknownEmailAndInactiveUser_ProduceIdenticalErrorMessage()
    {
        using var dbForInactive = TestDbContextFactory.Create();
        dbForInactive.Users.Add(new User
        {
            Id = 4, FullName = "Ghost", Email = "ghost@school.edu",
            PasswordHash = "hash", Role = UserRole.Student, IsActive = false
        });
        await dbForInactive.SaveChangesAsync();

        using var dbForUnknown = TestDbContextFactory.Create();

        var sutInactive = CreateSut(dbForInactive);
        var sutUnknown = CreateSut(dbForUnknown);

        var exInactive = await Assert.ThrowsAsync<UnauthorizedAppException>(() =>
            sutInactive.LoginAsync(new LoginRequestDto { Email = "ghost@school.edu", Password = "anything" }));

        var exUnknown = await Assert.ThrowsAsync<UnauthorizedAppException>(() =>
            sutUnknown.LoginAsync(new LoginRequestDto { Email = "nobody@school.edu", Password = "anything" }));

        Assert.Equal(exInactive.Message, exUnknown.Message);
    }
}
