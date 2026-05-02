using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TutorFlow.API.DTOs;
using TutorFlow.API.Services;
using TutorFlow.Core.Entities;
using TutorFlow.Core.Interfaces;
using TutorFlow.Infrastructure.Data;

namespace TutorFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubmissionsController : ControllerBase
{
    private readonly ISubmissionRepository _submissions;
    private readonly IAssignmentRepository _assignments;
    private readonly PistonService _piston;
    private readonly BadgeService _badges;
    private readonly AppDbContext _context;

    public SubmissionsController(
        ISubmissionRepository submissions,
        IAssignmentRepository assignments,
        PistonService piston,
        BadgeService badges,
        AppDbContext context)
    {
        _submissions = submissions;
        _assignments = assignments;
        _piston = piston;
        _badges = badges;
        _context = context;
    }

    [HttpGet("student/{studentId:int}")]
    public async Task<ActionResult<IEnumerable<SubmissionResponseDto>>> GetByStudent(int studentId)
    {
        var submissions = await _submissions.GetByStudentAsync(studentId);
        return Ok(submissions.Select(MapToDto));
    }

    [HttpGet("assignment/{assignmentId:int}")]
    public async Task<ActionResult<IEnumerable<SubmissionResponseDto>>> GetByAssignment(int assignmentId)
    {
        var submissions = await _submissions.GetByAssignmentAsync(assignmentId);
        return Ok(submissions.Select(MapToDto));
    }

    [HttpPost]
    public async Task<ActionResult<SubmissionResponseDto>> Submit([FromBody] CreateSubmissionDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var role = User.FindFirstValue(ClaimTypes.Role);

        // ── Tutor test run (studentId = 0) ────────────────────────────────
        // Just execute the code and return output — no DB record, no XP
        if (role == "Tutor" && dto.StudentId == 0)
        {
            var assignment = await _assignments.GetByIdAsync(dto.AssignmentId, tutorId: string.Empty);
            if (assignment is null) return NotFound(new { message = "Assignment not found." });

            var testResult = await _piston.ExecuteAsync(assignment.Language, dto.Code);
            return Ok(new SubmissionResponseDto(
                Id: 0,
                StudentId: 0,
                StudentName: "Tutor Preview",
                AssignmentId: dto.AssignmentId,
                AssignmentTitle: assignment.Title,
                Code: dto.Code,
                Output: testResult.HasError ? testResult.Error : testResult.Output,
                IsCorrect: false,
                SubmittedAt: DateTime.UtcNow));
        }

        // ── Student submission ─────────────────────────────────────────────
        int resolvedStudentId = dto.StudentId;

        if (role == "Student")
        {
            var studentRecord = await _context.Students
                .FirstOrDefaultAsync(s => s.ApplicationUserId == userId);

            if (studentRecord is null)
                return BadRequest(new { message = "Your account hasn't been linked to a student record yet. Ask your tutor." });

            resolvedStudentId = studentRecord.Id;
        }

        var assignmentForStudent = await _assignments.GetByIdAsync(dto.AssignmentId, tutorId: string.Empty);
        if (assignmentForStudent is null) return NotFound(new { message = "Assignment not found." });

        var result = await _piston.ExecuteAsync(assignmentForStudent.Language, dto.Code);

        var isCorrect = false;
        if (!result.HasError && !string.IsNullOrWhiteSpace(assignmentForStudent.ExpectedOutput))
        {
            isCorrect = string.Equals(
                result.Output.Trim(),
                assignmentForStudent.ExpectedOutput.Trim(),
                StringComparison.OrdinalIgnoreCase);
        }

        var submission = new Submission
        {
            StudentId = resolvedStudentId,
            AssignmentId = dto.AssignmentId,
            Code = dto.Code,
            Output = result.HasError ? result.Error : result.Output,
            IsCorrect = isCorrect
        };

        var created = await _submissions.CreateAsync(submission);
        await _badges.AwardBadgesAsync(resolvedStudentId);

        return CreatedAtAction(nameof(GetByStudent),
            new { studentId = created.StudentId },
            MapToDto(created));
    }

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
