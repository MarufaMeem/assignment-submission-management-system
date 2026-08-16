using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.DTOs.Assignments;
using AssignmentSystem.Api.DTOs.Submissions;
using AssignmentSystem.Api.Entities;
using AssignmentSystem.Api.Common;
using AssignmentSystem.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Api.Services.Implementations;

public class AssignmentService : IAssignmentService
{
    private readonly ApplicationDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AssignmentService(ApplicationDbContext db, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<AssignmentResponseDto> CreateAsync(CreateAssignmentDto dto, int teacherId, CancellationToken ct)
    {
        // Must verify teacher is assigned to this exact class/subject
        var isAuthorized = await _db.TeacherClassSubjects.AnyAsync(
            t => t.TeacherId == teacherId && t.ClassId == dto.ClassId && t.SubjectId == dto.SubjectId, ct);

        if (!isAuthorized)
        {
            throw new UnauthorizedAppException("You are not authorized to create assignments for this class and subject.");
        }

        var assignment = new Assignment
        {
            Title = dto.Title,
            Description = dto.Description,
            ClassId = dto.ClassId,
            SubjectId = dto.SubjectId,
            Deadline = dto.Deadline.ToUniversalTime(),
            MaxMarks = dto.MaxMarks,
            AllowLateSubmission = dto.AllowLateSubmission,
            CreatedByTeacherId = teacherId,
            Status = AssignmentStatus.Draft,
            CreatedAt = _dateTimeProvider.UtcNow,
            UpdatedAt = _dateTimeProvider.UtcNow
        };

        _db.Assignments.Add(assignment);
        await _db.SaveChangesAsync(ct);

        return await GetByIdAsync(assignment.Id, teacherId, ct);
    }

    public async Task<AssignmentResponseDto> UpdateAsync(int id, UpdateAssignmentDto dto, int teacherId, CancellationToken ct)
    {
        var assignment = await _db.Assignments.FirstOrDefaultAsync(a => a.Id == id && a.CreatedByTeacherId == teacherId, ct)
                         ?? throw new NotFoundAppException($"Assignment with id {id} was not found or you are not authorized.");

        if (assignment.Status == AssignmentStatus.Published)
        {
            throw new ValidationAppException("Cannot update a published assignment. Consider assumptions: We disallow editing after it is published to students.");
        }

        assignment.Title = dto.Title;
        assignment.Description = dto.Description;
        assignment.Deadline = dto.Deadline.ToUniversalTime();
        assignment.MaxMarks = dto.MaxMarks;
        assignment.AllowLateSubmission = dto.AllowLateSubmission;
        assignment.UpdatedAt = _dateTimeProvider.UtcNow;

        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(assignment.Id, teacherId, ct);
    }

    public async Task DeleteAsync(int id, int teacherId, CancellationToken ct)
    {
        var assignment = await _db.Assignments.FirstOrDefaultAsync(a => a.Id == id && a.CreatedByTeacherId == teacherId, ct)
                         ?? throw new NotFoundAppException($"Assignment with id {id} was not found or you are not authorized.");

        // Soft delete
        assignment.IsDeleted = true;
        assignment.UpdatedAt = _dateTimeProvider.UtcNow;
        
        await _db.SaveChangesAsync(ct);
    }

    public async Task<AssignmentResponseDto> PublishAsync(int id, int teacherId, CancellationToken ct)
    {
        var assignment = await _db.Assignments.FirstOrDefaultAsync(a => a.Id == id && a.CreatedByTeacherId == teacherId, ct)
                         ?? throw new NotFoundAppException($"Assignment with id {id} was not found or you are not authorized.");

        if (assignment.Status == AssignmentStatus.Published)
        {
            throw new ValidationAppException("Assignment is already published.");
        }

        assignment.Status = AssignmentStatus.Published;
        assignment.UpdatedAt = _dateTimeProvider.UtcNow;

        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(assignment.Id, teacherId, ct);
    }

    public async Task<List<AssignmentResponseDto>> GetByTeacherAsync(int teacherId, CancellationToken ct)
    {
        return await _db.Assignments
            .Where(a => a.CreatedByTeacherId == teacherId)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new AssignmentResponseDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                ClassId = a.ClassId,
                ClassName = a.Class.Name,
                SubjectId = a.SubjectId,
                SubjectName = a.Subject.Name,
                CreatedByTeacherId = a.CreatedByTeacherId,
                CreatedByTeacherName = a.CreatedByTeacher.FullName,
                Deadline = a.Deadline,
                MaxMarks = a.MaxMarks,
                Status = a.Status.ToString(),
                AllowLateSubmission = a.AllowLateSubmission,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            })
            .ToListAsync(ct);
    }

    public async Task<AssignmentResponseDto> GetByIdAsync(int id, int teacherId, CancellationToken ct)
    {
        var assignment = await _db.Assignments
            .Where(a => a.Id == id && a.CreatedByTeacherId == teacherId)
            .Select(a => new AssignmentResponseDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                ClassId = a.ClassId,
                ClassName = a.Class.Name,
                SubjectId = a.SubjectId,
                SubjectName = a.Subject.Name,
                CreatedByTeacherId = a.CreatedByTeacherId,
                CreatedByTeacherName = a.CreatedByTeacher.FullName,
                Deadline = a.Deadline,
                MaxMarks = a.MaxMarks,
                Status = a.Status.ToString(),
                AllowLateSubmission = a.AllowLateSubmission,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            })
            .FirstOrDefaultAsync(ct);

        return assignment ?? throw new NotFoundAppException($"Assignment with id {id} was not found or you are not authorized.");
    }

    public async Task<List<SubmissionResponseDto>> GetSubmissionsAsync(int assignmentId, int teacherId, CancellationToken ct)
    {
        // First check teacher owns the assignment
        var isAuthorized = await _db.Assignments.AnyAsync(a => a.Id == assignmentId && a.CreatedByTeacherId == teacherId, ct);
        if (!isAuthorized)
        {
            throw new NotFoundAppException($"Assignment with id {assignmentId} was not found or you are not authorized.");
        }

        return await _db.Submissions
            .Where(s => s.AssignmentId == assignmentId)
            .OrderByDescending(s => s.SubmittedAt)
            .Select(s => new SubmissionResponseDto
            {
                Id = s.Id,
                AssignmentId = s.AssignmentId,
                AssignmentTitle = s.Assignment.Title,
                StudentId = s.StudentId,
                StudentName = s.Student.FullName,
                AnswerText = s.AnswerText,
                SubmittedAt = s.SubmittedAt,
                Status = s.Status.ToString(),
                Marks = s.Marks,
                Feedback = s.Feedback,
                ReviewedByTeacherId = s.ReviewedByTeacherId,
                ReviewedAt = s.ReviewedAt
            })
            .ToListAsync(ct);
    }

    public async Task<SubmissionResponseDto> ReviewSubmissionAsync(int assignmentId, int submissionId, ReviewSubmissionDto dto, int teacherId, CancellationToken ct)
    {
        var submission = await _db.Submissions
            .Include(s => s.Assignment)
            .Include(s => s.Student)
            .FirstOrDefaultAsync(s => s.Id == submissionId && s.AssignmentId == assignmentId, ct)
            ?? throw new NotFoundAppException($"Submission with id {submissionId} for assignment {assignmentId} was not found.");

        if (submission.Assignment.CreatedByTeacherId != teacherId)
        {
            throw new UnauthorizedAppException("You are not authorized to review submissions for this assignment.");
        }

        if (dto.Marks > submission.Assignment.MaxMarks)
        {
            throw new ValidationAppException($"Marks cannot exceed the maximum marks of {submission.Assignment.MaxMarks}.");
        }

        submission.Marks = dto.Marks;
        submission.Feedback = dto.Feedback;
        submission.ReviewedByTeacherId = teacherId;
        submission.ReviewedAt = _dateTimeProvider.UtcNow;
        submission.Status = SubmissionStatus.Reviewed;

        await _db.SaveChangesAsync(ct);

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
