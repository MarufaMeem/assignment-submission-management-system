using System.Security.Claims;
using AssignmentSystem.Api.DTOs.Students;
using AssignmentSystem.Api.DTOs.Submissions;
using AssignmentSystem.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Controllers;

[ApiController]
[Route("api/students")]
[Authorize(Roles = "Student")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;

    public StudentsController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet("assignments")]
    public async Task<ActionResult<List<StudentAssignmentResponseDto>>> GetAssignments(CancellationToken ct)
    {
        var assignments = await _studentService.GetAvailableAssignmentsAsync(GetUserId(), ct);
        return Ok(assignments);
    }

    [HttpGet("assignments/{id:int}")]
    public async Task<ActionResult<StudentAssignmentResponseDto>> GetAssignmentDetails(int id, CancellationToken ct)
    {
        var assignment = await _studentService.GetAssignmentByIdAsync(GetUserId(), id, ct);
        return Ok(assignment);
    }

    [HttpPost("assignments/{assignmentId:int}/submissions")]
    public async Task<ActionResult<SubmissionResponseDto>> SubmitAnswer(
        int assignmentId, 
        [FromBody] CreateSubmissionDto dto, 
        CancellationToken ct)
    {
        var created = await _studentService.CreateSubmissionAsync(GetUserId(), assignmentId, dto, ct);
        return CreatedAtAction(nameof(GetMySubmission), new { assignmentId = created.AssignmentId }, created);
    }

    [HttpPut("assignments/{assignmentId:int}/submissions/{submissionId:int}")]
    public async Task<ActionResult<SubmissionResponseDto>> UpdateSubmission(
        int assignmentId, 
        int submissionId, 
        [FromBody] UpdateSubmissionDto dto, 
        CancellationToken ct)
    {
        var updated = await _studentService.UpdateSubmissionAsync(GetUserId(), assignmentId, submissionId, dto, ct);
        return Ok(updated);
    }

    [HttpGet("assignments/{assignmentId:int}/submissions/my")]
    public async Task<ActionResult<SubmissionResponseDto>> GetMySubmission(int assignmentId, CancellationToken ct)
    {
        var submission = await _studentService.GetMySubmissionAsync(GetUserId(), assignmentId, ct);
        return Ok(submission);
    }
}
