using AssignmentSystem.Api.Common;
using AssignmentSystem.Api.DTOs.Students;
using AssignmentSystem.Api.Entities;
using AssignmentSystem.Api.Services.Implementations;
using AssignmentSystem.Api.Services.Interfaces;
using AssignmentSystem.Tests.TestHelpers;
using Xunit;

namespace AssignmentSystem.Tests.Services;

public class StudentServiceTests
{
    private static StudentService CreateSut(AssignmentSystem.Api.Data.ApplicationDbContext db, IDateTimeProvider? timeProvider = null) =>
        new(db, timeProvider ?? new SystemDateTimeProvider());

    private static async Task<(Class classA, Class classB, User studentA, Assignment validAssignment, Assignment invalidAssignment)> SeedDataAsync(AssignmentSystem.Api.Data.ApplicationDbContext db)
    {
        var classA = new Class { Name = "Class A" };
        var classB = new Class { Name = "Class B" };
        var subject = new Subject { Name = "Math" };
        var teacher = new User { FullName = "T1", Email = "t1@school.edu", PasswordHash = "x", Role = UserRole.Teacher, IsActive = true };
        
        db.AddRange(classA, classB, subject, teacher);
        await db.SaveChangesAsync();

        var studentA = new User { FullName = "Student A", Email = "sa@school.edu", PasswordHash = "x", Role = UserRole.Student, ClassId = classA.Id, IsActive = true };
        db.Users.Add(studentA);
        
        var validAssignment = new Assignment
        {
            Title = "Valid",
            Description = "Desc",
            ClassId = classA.Id,
            SubjectId = subject.Id,
            CreatedByTeacherId = teacher.Id,
            MaxMarks = 100,
            Status = AssignmentStatus.Published,
            Deadline = DateTime.UtcNow.AddDays(1)
        };

        var invalidAssignment = new Assignment
        {
            Title = "Wrong Class",
            Description = "Desc",
            ClassId = classB.Id,
            SubjectId = subject.Id,
            CreatedByTeacherId = teacher.Id,
            MaxMarks = 100,
            Status = AssignmentStatus.Published,
            Deadline = DateTime.UtcNow.AddDays(1)
        };
        db.Assignments.AddRange(validAssignment, invalidAssignment);
        await db.SaveChangesAsync();

        return (classA, classB, studentA, validAssignment, invalidAssignment);
    }

    [Fact]
    public async Task GetAvailableAssignments_ReturnsOnlyClassAssignments()
    {
        using var db = TestDbContextFactory.Create();
        var (_, _, studentA, validAssignment, _) = await SeedDataAsync(db);
        var sut = CreateSut(db);

        var result = await sut.GetAvailableAssignmentsAsync(studentA.Id, default);

        Assert.Single(result);
        Assert.Equal(validAssignment.Id, result[0].Id);
    }

    [Fact]
    public async Task GetAssignmentById_WrongClass_ThrowsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        var (_, _, studentA, _, invalidAssignment) = await SeedDataAsync(db);
        var sut = CreateSut(db);

        var exception = await Assert.ThrowsAsync<NotFoundAppException>(() => 
            sut.GetAssignmentByIdAsync(studentA.Id, invalidAssignment.Id, default));
            
        Assert.Contains("was not found", exception.Message);
    }

    [Fact]
    public async Task CreateSubmission_PastDeadline_ThrowsValidation()
    {
        using var db = TestDbContextFactory.Create();
        var (_, _, studentA, validAssignment, _) = await SeedDataAsync(db);
        
        // Push deadline to the past
        validAssignment.Deadline = DateTime.UtcNow.AddSeconds(-1);
        await db.SaveChangesAsync();

        var sut = CreateSut(db);
        var dto = new CreateSubmissionDto { AnswerText = "My answer" };

        var exception = await Assert.ThrowsAsync<ValidationAppException>(() => 
            sut.CreateSubmissionAsync(studentA.Id, validAssignment.Id, dto, default));
            
        Assert.Contains("deadline has passed", exception.Message);
    }

    [Fact]
    public async Task UpdateSubmission_Reviewed_ThrowsValidation()
    {
        using var db = TestDbContextFactory.Create();
        var (_, _, studentA, validAssignment, _) = await SeedDataAsync(db);

        var submission = new Submission
        {
            AssignmentId = validAssignment.Id,
            StudentId = studentA.Id,
            AnswerText = "Initial",
            Status = SubmissionStatus.Reviewed,
            Marks = 50,
            ReviewedByTeacherId = validAssignment.CreatedByTeacherId
        };
        db.Submissions.Add(submission);
        await db.SaveChangesAsync();

        var sut = CreateSut(db);
        var dto = new UpdateSubmissionDto { AnswerText = "Updated" };

        var exception = await Assert.ThrowsAsync<ValidationAppException>(() => 
            sut.UpdateSubmissionAsync(studentA.Id, validAssignment.Id, submission.Id, dto, default));
            
        Assert.Contains("already been reviewed", exception.Message);
    }
}
