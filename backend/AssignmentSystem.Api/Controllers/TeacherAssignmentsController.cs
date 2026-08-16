using AssignmentSystem.Api.DTOs.TeacherAssignments;
using AssignmentSystem.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Controllers;

[ApiController]
[Route("api/teacher-assignments")]
[Authorize(Roles = "Admin")]
public class TeacherAssignmentsController : ControllerBase
{
    private readonly ITeacherAssignmentService _service;

    public TeacherAssignmentsController(ITeacherAssignmentService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<TeacherAssignmentDto>>> GetAll([FromQuery] int? teacherId, CancellationToken ct)
    {
        return Ok(await _service.GetAllAsync(teacherId, ct));
    }

    [HttpPost]
    public async Task<ActionResult<TeacherAssignmentDto>> Create([FromBody] CreateTeacherAssignmentDto dto, CancellationToken ct)
    {
        var created = await _service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetAll), new { teacherId = created.TeacherId }, created);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }
}
