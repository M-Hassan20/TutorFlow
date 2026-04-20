using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TutorFlow.API.DTOs;
using TutorFlow.Infrastructure;
using TutorFlow.Core.Entities;
using TutorFlow.Core.Interfaces;

namespace TutorFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Tutor")]
public class AssignmentsController : ControllerBase
{
    private readonly IAssignmentRepository _assignments;

    public AssignmentsController(IAssignmentRepository assignments)
    {
        _assignments = assignments;
    }

    /// <summary>Get all assignments created by the logged-in tutor</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AssignmentResponseDto>>> GetAll()
    {
        var tutorId = GetTutorId();
        var assignments = await _assignments.GetAllByTutorAsync(tutorId);
        return Ok(assignments.Select(MapToDto));
    }

    /// <summary>Get a single assignment by ID</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AssignmentResponseDto>> GetById(int id)
    {
        var tutorId = GetTutorId();
        var assignment = await _assignments.GetByIdAsync(id, tutorId);
        return assignment is null ? NotFound() : Ok(MapToDto(assignment));
    }

    /// <summary>Create a new assignment</summary>
    [HttpPost]
    public async Task<ActionResult<AssignmentResponseDto>> Create([FromBody] CreateAssignmentDto dto)
    {
        var tutorId = GetTutorId();
        var assignment = new Assignment
        {
            Title = dto.Title,
            Description = dto.Description,
            StarterCode = dto.StarterCode,
            Language = dto.Language,
            XPReward = dto.XPReward,
            DueDate = dto.DueDate,
            TutorId = tutorId
        };

        var created = await _assignments.CreateAsync(assignment);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToDto(created));
    }

    /// <summary>Update an existing assignment</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<AssignmentResponseDto>> Update(int id, [FromBody] UpdateAssignmentDto dto)
    {
        var tutorId = GetTutorId();
        var assignment = new Assignment
        {
            Id = id,
            Title = dto.Title,
            Description = dto.Description,
            StarterCode = dto.StarterCode,
            Language = dto.Language,
            XPReward = dto.XPReward,
            DueDate = dto.DueDate,
            TutorId = tutorId
        };

        var updated = await _assignments.UpdateAsync(assignment);
        return updated is null ? NotFound() : Ok(MapToDto(updated));
    }

    /// <summary>Delete an assignment</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var tutorId = GetTutorId();
        var deleted = await _assignments.DeleteAsync(id, tutorId);
        return deleted ? NoContent() : NotFound();
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private string GetTutorId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("User ID not found in token.");

    private static AssignmentResponseDto MapToDto(Assignment a) => new(
        Id: a.Id,
        Title: a.Title,
        Description: a.Description,
        StarterCode: a.StarterCode,
        Language: a.Language,
        XPReward: a.XPReward,
        CreatedAt: a.CreatedAt,
        DueDate: a.DueDate,
        StudentCount: a.Students?.Count ?? 0
    );
}
