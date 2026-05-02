using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TutorFlow.API.DTOs;
using TutorFlow.Core.Entities;
using TutorFlow.Core.Interfaces;

namespace TutorFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // All roles can access — individual actions restrict further
public class AssignmentsController : ControllerBase
{
    private readonly IAssignmentRepository _assignments;

    public AssignmentsController(IAssignmentRepository assignments)
    {
        _assignments = assignments;
    }

    /// <summary>
    /// Get assignments. Tutors get their own; students get all assignments
    /// linked to their tutor (filtered in repository by empty tutorId = all).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AssignmentResponseDto>>> GetAll()
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // Tutors see only their own assignments
        var tutorId = role == "Tutor" ? userId : string.Empty;
        var assignments = await _assignments.GetAllByTutorAsync(tutorId);
        return Ok(assignments.Select(MapToDto));
    }

    /// <summary>Get a single assignment by ID — available to all roles</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AssignmentResponseDto>> GetById(int id)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var tutorId = role == "Tutor" ? userId : string.Empty;

        var assignment = await _assignments.GetByIdAsync(id, tutorId);
        return assignment is null ? NotFound() : Ok(MapToDto(assignment));
    }

    /// <summary>Create a new assignment — Tutor only</summary>
    [HttpPost]
    [Authorize(Roles = "Tutor")]
    public async Task<ActionResult<AssignmentResponseDto>> Create([FromBody] CreateAssignmentDto dto)
    {
        var tutorId = GetUserId();
        var assignment = new Assignment
        {
            Title = dto.Title,
            Description = dto.Description,
            StarterCode = dto.StarterCode,
            Language = dto.Language,
            XPReward = dto.XPReward,
            ExpectedOutput = dto.ExpectedOutput?.Trim(),
            DueDate = dto.DueDate,
            TutorId = tutorId
        };

        var created = await _assignments.CreateAsync(assignment);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToDto(created));
    }

    /// <summary>Update an existing assignment — Tutor only</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Tutor")]
    public async Task<ActionResult<AssignmentResponseDto>> Update(int id, [FromBody] UpdateAssignmentDto dto)
    {
        var tutorId = GetUserId();
        var assignment = new Assignment
        {
            Id = id,
            Title = dto.Title,
            Description = dto.Description,
            StarterCode = dto.StarterCode,
            Language = dto.Language,
            XPReward = dto.XPReward,
            ExpectedOutput = dto.ExpectedOutput?.Trim(),
            DueDate = dto.DueDate,
            TutorId = tutorId
        };

        var updated = await _assignments.UpdateAsync(assignment);
        return updated is null ? NotFound() : Ok(MapToDto(updated));
    }

    /// <summary>Delete an assignment — Tutor only</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Tutor")]
    public async Task<IActionResult> Delete(int id)
    {
        var tutorId = GetUserId();
        var deleted = await _assignments.DeleteAsync(id, tutorId);
        return deleted ? NoContent() : NotFound();
    }

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("User ID not found in token.");

    private static AssignmentResponseDto MapToDto(Assignment a) => new(
        Id: a.Id,
        Title: a.Title,
        Description: a.Description,
        StarterCode: a.StarterCode,
        Language: a.Language,
        XPReward: a.XPReward,
        ExpectedOutput: a.ExpectedOutput,
        CreatedAt: a.CreatedAt,
        DueDate: a.DueDate,
        StudentCount: a.Students?.Count ?? 0
    );
}
