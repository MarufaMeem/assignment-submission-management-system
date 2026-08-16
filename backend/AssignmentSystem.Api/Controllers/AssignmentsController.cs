using System.Security.Claims;
using AssignmentSystem.Api.DTOs.Assignments;
using AssignmentSystem.Api.DTOs.Submissions;
using AssignmentSystem.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Controllers;

[ApiController]
[Route("api/assignments")]
[Authorize(Roles = "Teacher")]
public class AssignmentsController : ControllerBase
{
    private readonly IAssignmentService _assignmentService;

    public AssignmentsController(IAssignmentService assignmentService)
    {
        _assignmentService = assignmentService;
    }

    private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpPost]
    public async Task<ActionResult<AssignmentResponseDto>> Create([FromBody] CreateAssignmentDto dto, CancellationToken ct)
    {
        var created = await _assignmentService.CreateAsync(dto, GetUserId(), ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AssignmentResponseDto>> Update(int id, [FromBody] UpdateAssignmentDto dto, CancellationToken ct)
    {
        var updated = await _assignmentService.UpdateAsync(id, dto, GetUserId(), ct);
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _assignmentService.DeleteAsync(id, GetUserId(), ct);
        return NoContent();
    }

    [HttpPost("{id:int}/publish")]
    public async Task<ActionResult<AssignmentResponseDto>> Publish(int id, CancellationToken ct)
    {
        var published = await _assignmentService.PublishAsync(id, GetUserId(), ct);
        return Ok(published);
    }

    [HttpGet]
    public async Task<ActionResult<List<AssignmentResponseDto>>> GetByTeacher(CancellationToken ct)
    {
        var assignments = await _assignmentService.GetByTeacherAsync(GetUserId(), ct);
        return Ok(assignments);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AssignmentResponseDto>> GetById(int id, CancellationToken ct)
    {
        var assignment = await _assignmentService.GetByIdAsync(id, GetUserId(), ct);
        return Ok(assignment);
    }

    [HttpGet("{id:int}/submissions")]
    public async Task<ActionResult<List<SubmissionResponseDto>>> GetSubmissions(int id, CancellationToken ct)
    {
        var submissions = await _assignmentService.GetSubmissionsAsync(id, GetUserId(), ct);
        return Ok(submissions);
    }

    [HttpPost("{assignmentId:int}/submissions/{submissionId:int}/review")]
    public async Task<ActionResult<SubmissionResponseDto>> ReviewSubmission(
        int assignmentId, 
        int submissionId, 
        [FromBody] ReviewSubmissionDto dto, 
        CancellationToken ct)
    {
        var reviewed = await _assignmentService.ReviewSubmissionAsync(assignmentId, submissionId, dto, GetUserId(), ct);
        return Ok(reviewed);
    }
}
