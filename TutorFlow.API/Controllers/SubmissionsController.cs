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
[Authorize]
public class SubmissionsController : ControllerBase
{
    private readonly ISubmissionRepository _submissions;

    public SubmissionsController(ISubmissionRepository submissions)
    {
        _submissions = submissions;
    }

    /// <summary>Get all submissions for a specific student</summary>
    [HttpGet("student/{studentId:int}")]
    public async Task<ActionResult<IEnumerable<SubmissionResponseDto>>> GetByStudent(int studentId)
    {
        var submissions = await _submissions.GetByStudentAsync(studentId);
        return Ok(submissions.Select(MapToDto));
    }

    /// <summary>Get all submissions for a specific assignment</summary>
    [HttpGet("assignment/{assignmentId:int}")]
    public async Task<ActionResult<IEnumerable<SubmissionResponseDto>>> GetByAssignment(int assignmentId)
    {
        var submissions = await _submissions.GetByAssignmentAsync(assignmentId);
        return Ok(submissions.Select(MapToDto));
    }

    /// <summary>Submit code for an assignment (code execution handled in Phase 2)</summary>
    [HttpPost]
    public async Task<ActionResult<SubmissionResponseDto>> Submit([FromBody] CreateSubmissionDto dto)
    {
        // NOTE: In Phase 2, this will call the Piston API to execute the code
        // and populate Output + IsCorrect automatically.
        // For now, we store the raw submission.
        var submission = new Submission
        {
            StudentId = dto.StudentId,
            AssignmentId = dto.AssignmentId,
            Code = dto.Code,
            Output = null,
            IsCorrect = false
        };

        var created = await _submissions.CreateAsync(submission);
        return CreatedAtAction(nameof(GetByStudent),
            new { studentId = created.StudentId },
            MapToDto(created));
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private static SubmissionResponseDto MapToDto(Submission s) => new(
        Id: s.Id,
        StudentId: s.StudentId,
        StudentName: s.Student is not null
            ? $"{s.Student.FirstName} {s.Student.LastName}"
            : string.Empty,
        AssignmentId: s.AssignmentId,
        AssignmentTitle: s.Assignment?.Title ?? string.Empty,
        Code: s.Code,
        Output: s.Output,
        IsCorrect: s.IsCorrect,
        SubmittedAt: s.SubmittedAt
    );
}
