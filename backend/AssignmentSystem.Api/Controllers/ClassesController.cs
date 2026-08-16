using AssignmentSystem.Api.DTOs.Classes;
using AssignmentSystem.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Controllers;

[ApiController]
[Route("api/classes")]
[Authorize] // any authenticated role can read; write actions are further restricted below
public class ClassesController : ControllerBase
{
    private readonly IClassService _classService;

    public ClassesController(IClassService classService)
    {
        _classService = classService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ClassDto>>> GetAll(CancellationToken ct)
    {
        return Ok(await _classService.GetAllAsync(ct));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClassDto>> GetById(int id, CancellationToken ct)
    {
        return Ok(await _classService.GetByIdAsync(id, ct));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ClassDto>> Create([FromBody] UpsertClassDto dto, CancellationToken ct)
    {
        var created = await _classService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ClassDto>> Update(int id, [FromBody] UpsertClassDto dto, CancellationToken ct)
    {
        return Ok(await _classService.UpdateAsync(id, dto, ct));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _classService.DeleteAsync(id, ct);
        return NoContent();
    }
}
