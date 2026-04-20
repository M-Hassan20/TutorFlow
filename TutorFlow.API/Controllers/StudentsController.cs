using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TutorFlow.API.DTOs;
using TutorFlow.Infrastructure;
using TutorFlow.Core.Interfaces;
using TutorFlow.Core.Entities;

namespace TutorFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Tutor")]
public class StudentsController : ControllerBase
{
    private readonly IStudentRepository _students;

    public StudentsController(IStudentRepository students)
    {
        _students = students;
    }

    /// <summary>Get all students belonging to the logged-in tutor</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<StudentResponseDto>>> GetAll()
    {
        var tutorId = GetTutorId();
        var students = await _students.GetAllByTutorAsync(tutorId);
        return Ok(students.Select(MapToDto));
    }

    /// <summary>Get a single student by ID</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<StudentResponseDto>> GetById(int id)
    {
        var tutorId = GetTutorId();
        var student = await _students.GetByIdAsync(id, tutorId);
        return student is null ? NotFound() : Ok(MapToDto(student));
    }

    /// <summary>Create a new student under the logged-in tutor</summary>
    [HttpPost]
    public async Task<ActionResult<StudentResponseDto>> Create([FromBody] CreateStudentDto dto)
    {
        var tutorId = GetTutorId();
        var student = new Student
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Age = dto.Age,
            ParentEmail = dto.ParentEmail,
            TutorId = tutorId
        };

        var created = await _students.CreateAsync(student);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToDto(created));
    }

    /// <summary>Update a student's details</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<StudentResponseDto>> Update(int id, [FromBody] UpdateStudentDto dto)
    {
        var tutorId = GetTutorId();
        var student = new Student
        {
            Id = id,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Age = dto.Age,
            ParentEmail = dto.ParentEmail,
            TutorId = tutorId
        };

        var updated = await _students.UpdateAsync(student);
        return updated is null ? NotFound() : Ok(MapToDto(updated));
    }

    /// <summary>Soft-delete a student</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var tutorId = GetTutorId();
        var deleted = await _students.DeleteAsync(id, tutorId);
        return deleted ? NoContent() : NotFound();
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private string GetTutorId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("User ID not found in token.");

    private static StudentResponseDto MapToDto(Student s) => new(
        Id: s.Id,
        FirstName: s.FirstName,
        LastName: s.LastName,
        Age: s.Age,
        ParentEmail: s.ParentEmail,
        XP: s.XP,
        EnrolledAt: s.EnrolledAt,
        IsActive: s.IsActive
    );
}
