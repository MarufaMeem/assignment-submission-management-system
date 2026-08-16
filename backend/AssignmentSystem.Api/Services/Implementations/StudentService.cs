using AssignmentSystem.Api.Common;
using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.DTOs.Students;
using AssignmentSystem.Api.DTOs.Submissions;
using AssignmentSystem.Api.Entities;
using AssignmentSystem.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Api.Services.Implementations;

public class StudentService : IStudentService
{
    private readonly ApplicationDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;

    public StudentService(ApplicationDbContext db, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
    }

    private async Task<int> GetStudentClassIdAsync(int studentId, CancellationToken ct)
    {
        var student = await _db.Users.FirstOrDefaultAsync(u => u.Id == studentId && u.Role == UserRole.Student, ct);
        if (student == null || student.ClassId == null)
            throw new UnauthorizedAppException("Only students mapped to a specific class can access assignments.");
            
        return student.ClassId.Value;
    }

    public async Task<List<StudentAssignmentResponseDto>> GetAvailableAssignmentsAsync(int studentId, CancellationToken ct)
    {
        var classId = await GetStudentClassIdAsync(studentId, ct);

        return await _db.Assignments
            .Where(a => a.ClassId == classId && a.Status == AssignmentStatus.Published && !a.IsDeleted)
            .OrderBy(a => a.Deadline)
            .Select(a => new StudentAssignmentResponseDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                ClassName = a.Class.Name,
                SubjectName = a.Subject.Name,
                CreatedByTeacherName = a.CreatedByTeacher.FullName,
                Deadline = a.Deadline,
                MaxMarks = a.MaxMarks,
                AllowLateSubmission = a.AllowLateSubmission
            })
            .ToListAsync(ct);
    }

    public async Task<StudentAssignmentResponseDto> GetAssignmentByIdAsync(int studentId, int assignmentId, CancellationToken ct)
    {
        var classId = await GetStudentClassIdAsync(studentId, ct);

        var assignment = await _db.Assignments
            .Where(a => a.Id == assignmentId && a.ClassId == classId && a.Status == AssignmentStatus.Published && !a.IsDeleted)
            .Select(a => new StudentAssignmentResponseDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                ClassName = a.Class.Name,
                SubjectName = a.Subject.Name,
                CreatedByTeacherName = a.CreatedByTeacher.FullName,
                Deadline = a.Deadline,
                MaxMarks = a.MaxMarks,
                AllowLateSubmission = a.AllowLateSubmission
            })
            .FirstOrDefaultAsync(ct);

        return assignment ?? throw new NotFoundAppException($"Assignment with id {assignmentId} was not found, is not published, or does not belong to your class.");
    }

    public async Task<SubmissionResponseDto> CreateSubmissionAsync(int studentId, int assignmentId, CreateSubmissionDto dto, CancellationToken ct)
    {
        var classId = await GetStudentClassIdAsync(studentId, ct);

        var assignment = await _db.Assignments
            .FirstOrDefaultAsync(a => a.Id == assignmentId && a.ClassId == classId && a.Status == AssignmentStatus.Published && !a.IsDeleted, ct)
            ?? throw new NotFoundAppException("Assignment not found for your class.");

        // Enforce Deadline
        var now = _dateTimeProvider.UtcNow;
        if (now > assignment.Deadline && !assignment.AllowLateSubmission)
        {
            throw new ValidationAppException("Cannot submit this assignment because the deadline has passed.");
        }

        // Check if existing submission
        var existing = await _db.Submissions.AnyAsync(s => s.AssignmentId == assignmentId && s.StudentId == studentId, ct);
        if (existing)
        {
            throw new ConflictAppException("You have already submitted an answer to this assignment. Consider updating the existing submission instead if permitted.");
        }

        var submission = new Submission
        {
            AssignmentId = assignmentId,
            StudentId = studentId,
            AnswerText = dto.AnswerText,
            SubmittedAt = now,
            Status = SubmissionStatus.Submitted
        };

        _db.Submissions.Add(submission);
        await _db.SaveChangesAsync(ct);

        return await GetMySubmissionAsync(studentId, assignmentId, ct);
    }

    public async Task<SubmissionResponseDto> UpdateSubmissionAsync(int studentId, int assignmentId, int submissionId, UpdateSubmissionDto dto, CancellationToken ct)
    {
        var classId = await GetStudentClassIdAsync(studentId, ct);

        var assignment = await _db.Assignments
            .FirstOrDefaultAsync(a => a.Id == assignmentId && a.ClassId == classId && a.Status == AssignmentStatus.Published && !a.IsDeleted, ct)
            ?? throw new NotFoundAppException("Assignment not found for your class.");

        var submission = await _db.Submissions
            .FirstOrDefaultAsync(s => s.Id == submissionId && s.AssignmentId == assignmentId && s.StudentId == studentId, ct)
            ?? throw new NotFoundAppException("Submission not found.");

        if (submission.Status == SubmissionStatus.Reviewed)
        {
            throw new ValidationAppException("Cannot update your submission because it has already been reviewed by a teacher.");
        }

        var now = _dateTimeProvider.UtcNow;
        if (now > assignment.Deadline && !assignment.AllowLateSubmission)
        {
            throw new ValidationAppException("Cannot update your submission because the deadline has passed.");
        }

        submission.AnswerText = dto.AnswerText;
        submission.SubmittedAt = now; 
        
        await _db.SaveChangesAsync(ct);

        return await GetMySubmissionAsync(studentId, assignmentId, ct);
    }

    public async Task<SubmissionResponseDto> GetMySubmissionAsync(int studentId, int assignmentId, CancellationToken ct)
    {
        var submission = await _db.Submissions
            .Include(s => s.Assignment)
            .Include(s => s.Student)
            .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == studentId, ct)
            ?? throw new NotFoundAppException("You have not submitted an answer for this assignment.");

        return new SubmissionResponseDto
        {
            Id = submission.Id,
            AssignmentId = submission.AssignmentId,
            AssignmentTitle = submission.Assignment.Title,
            StudentId = submission.StudentId,
            StudentName = submission.Student.FullName,
            AnswerText = submission.AnswerText,
            SubmittedAt = submission.SubmittedAt,
            Status = submission.Status.ToString(),
            Marks = submission.Marks,
            Feedback = submission.Feedback,
            ReviewedByTeacherId = submission.ReviewedByTeacherId,
            ReviewedAt = submission.ReviewedAt
        };
    }
}
