using AssignmentSystem.Api.DTOs.Subjects;
using AssignmentSystem.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Controllers;

[ApiController]
[Route("api/subjects")]
[Authorize]
public class SubjectsController : ControllerBase
{
    private readonly ISubjectService _subjectService;

    public SubjectsController(ISubjectService subjectService)
    {
        _subjectService = subjectService;
    }

    [HttpGet]
    public async Task<ActionResult<List<SubjectDto>>> GetAll(CancellationToken ct)
    {
        return Ok(await _subjectService.GetAllAsync(ct));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SubjectDto>> GetById(int id, CancellationToken ct)
    {
        return Ok(await _subjectService.GetByIdAsync(id, ct));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SubjectDto>> Create([FromBody] UpsertSubjectDto dto, CancellationToken ct)
    {
        var created = await _subjectService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SubjectDto>> Update(int id, [FromBody] UpsertSubjectDto dto, CancellationToken ct)
    {
        return Ok(await _subjectService.UpdateAsync(id, dto, ct));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _subjectService.DeleteAsync(id, ct);
        return NoContent();
    }
}
