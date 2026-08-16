using AssignmentSystem.Api.Common;
using AssignmentSystem.Api.DTOs.Assignments;
using AssignmentSystem.Api.DTOs.Submissions;
using AssignmentSystem.Api.Entities;
using AssignmentSystem.Api.Exceptions;
using AssignmentSystem.Api.Services.Implementations;
using AssignmentSystem.Tests.TestHelpers;
using Xunit;

namespace AssignmentSystem.Tests.Services;

public class AssignmentServiceTests
{
    private static AssignmentService CreateSut(AssignmentSystem.Api.Data.ApplicationDbContext db) =>
        new(db, new SystemDateTimeProvider());

    private static async Task<(Class cls, Subject subj, User teacher)> SeedBasicsAsync(AssignmentSystem.Api.Data.ApplicationDbContext db)
    {
        var cls = new Class { Name = "Class A" };
        var subj = new Subject { Name = "Math" };
        var teacher = new User { FullName = "Teacher One", Email = "t1@school.edu", PasswordHash = "x", Role = UserRole.Teacher, IsActive = true };
        db.AddRange(cls, subj, teacher);
        await db.SaveChangesAsync();
        return (cls, subj, teacher);
    }

    [Fact]
    public async Task CreateAsync_UnauthorizedTeacher_ThrowsUnauthorized()
    {
        using var db = TestDbContextFactory.Create();
        var (cls, subj, teacher) = await SeedBasicsAsync(db);

        // Teacher is not assigned to Class A / Math
        var sut = CreateSut(db);
        var dto = new CreateAssignmentDto
        {
            Title = "Test",
            Description = "Desc",
            ClassId = cls.Id,
            SubjectId = subj.Id,
            Deadline = DateTime.UtcNow.AddDays(1),
            MaxMarks = 100
        };

        var exception = await Assert.ThrowsAsync<UnauthorizedAppException>(() => sut.CreateAsync(dto, teacher.Id, default));
        Assert.Contains("not authorized", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_AuthorizedTeacher_Succeeds()
    {
        using var db = TestDbContextFactory.Create();
        var (cls, subj, teacher) = await SeedBasicsAsync(db);
        db.TeacherClassSubjects.Add(new TeacherClassSubject { TeacherId = teacher.Id, ClassId = cls.Id, SubjectId = subj.Id });
        await db.SaveChangesAsync();

        var sut = CreateSut(db);
        var dto = new CreateAssignmentDto
        {
            Title = "Test",
            Description = "Desc",
            ClassId = cls.Id,
            SubjectId = subj.Id,
            Deadline = DateTime.UtcNow.AddDays(1),
            MaxMarks = 100
        };

        var result = await sut.CreateAsync(dto, teacher.Id, default);

        Assert.NotNull(result);
        Assert.Equal("Test", result.Title);
        Assert.Equal(AssignmentStatus.Draft.ToString(), result.Status); // Defaults to Draft
    }

    [Fact]
    public async Task PublishAsync_ValidAssignment_Publishes()
    {
        using var db = TestDbContextFactory.Create();
        var (cls, subj, teacher) = await SeedBasicsAsync(db);
        var assignment = new Assignment
        {
            Title = "A1",
            Description = "-",
            ClassId = cls.Id,
            SubjectId = subj.Id,
            CreatedByTeacherId = teacher.Id,
            MaxMarks = 10,
            Status = AssignmentStatus.Draft
        };
        db.Assignments.Add(assignment);
        await db.SaveChangesAsync();

        var sut = CreateSut(db);
        var result = await sut.PublishAsync(assignment.Id, teacher.Id, default);

        Assert.Equal(AssignmentStatus.Published.ToString(), result.Status);
    }

    [Fact]
    public async Task ReviewSubmissionAsync_MarksExceedMax_ThrowsValidation()
    {
        using var db = TestDbContextFactory.Create();
        var (cls, subj, teacher) = await SeedBasicsAsync(db);
        var student = new User { FullName = "S1", Email = "s1@school.edu", PasswordHash = "x", Role = UserRole.Student, IsActive = true };
        db.Users.Add(student);

        var assignment = new Assignment
        {
            Title = "A1",
            ClassId = cls.Id,
            SubjectId = subj.Id,
            CreatedByTeacherId = teacher.Id,
            MaxMarks = 50,
            Status = AssignmentStatus.Published
        };
        db.Assignments.Add(assignment);
        
        var submission = new Submission
        {
            Assignment = assignment,
            Student = student,
            AnswerText = "My answer"
        };
        db.Submissions.Add(submission);
        await db.SaveChangesAsync();

        var sut = CreateSut(db);
        var dto = new ReviewSubmissionDto { Marks = 60, Feedback = "Good" }; // 60 > 50

        var exception = await Assert.ThrowsAsync<ValidationAppException>(() => 
            sut.ReviewSubmissionAsync(assignment.Id, submission.Id, dto, teacher.Id, default));
            
        Assert.Contains("cannot exceed the maximum marks", exception.Message);
    }
    
    [Fact]
    public async Task ReviewSubmissionAsync_ValidReview_Succeeds()
    {
        using var db = TestDbContextFactory.Create();
        var (cls, subj, teacher) = await SeedBasicsAsync(db);
        var student = new User { FullName = "S1", Email = "s1@school.edu", PasswordHash = "x", Role = UserRole.Student, IsActive = true };
        db.Users.Add(student);

        var assignment = new Assignment
        {
            Title = "A1",
            ClassId = cls.Id,
            SubjectId = subj.Id,
            CreatedByTeacherId = teacher.Id,
            MaxMarks = 50,
            Status = AssignmentStatus.Published
        };
        db.Assignments.Add(assignment);
        
        var submission = new Submission
        {
            Assignment = assignment,
            Student = student,
            AnswerText = "My answer"
        };
        db.Submissions.Add(submission);
        await db.SaveChangesAsync();

        var sut = CreateSut(db);
        var dto = new ReviewSubmissionDto { Marks = 45, Feedback = "Great job!" };

        var result = await sut.ReviewSubmissionAsync(assignment.Id, submission.Id, dto, teacher.Id, default);

        Assert.Equal(45m, result.Marks);
        Assert.Equal("Great job!", result.Feedback);
        Assert.Equal(teacher.Id, result.ReviewedByTeacherId);
        Assert.Equal(SubmissionStatus.Reviewed.ToString(), result.Status);
    }
}
