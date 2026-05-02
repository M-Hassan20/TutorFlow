using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TutorFlow.API.DTOs;
using TutorFlow.API.Services;
using TutorFlow.Core.Enums;
using TutorFlow.Infrastructure.Data;

namespace TutorFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProgressController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly BadgeService _badges;
    private readonly ILogger<ProgressController> _logger;

    public ProgressController(
        AppDbContext context,
        BadgeService badges,
        ILogger<ProgressController> logger)
    {
        _context = context;
        _badges = badges;
        _logger = logger;
    }

    [HttpGet("student/{studentId:int}")]
    public async Task<ActionResult<StudentProgressDto>> GetProgress(int studentId)
    {
        try
        {
            var student = await _context.Students
                .Include(s => s.Badges)
                .Include(s => s.Assignments)
                .Include(s => s.Submissions).ThenInclude(sub => sub.Assignment)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student is null) return NotFound(new { message = "Student not found." });

            await _badges.AwardBadgesAsync(studentId);
            await _context.Entry(student).Collection(s => s.Badges).LoadAsync();

            var totalAssignments = student.Assignments.Count;
            var completedAssignments = student.Submissions
                .Where(s => s.IsCorrect)
                .Select(s => s.AssignmentId)
                .Distinct()
                .Count();

            var level = student.XP / 100;
            var xpIntoLevel = student.XP % 100;

            return Ok(new StudentProgressDto(
                StudentId: student.Id,
                FirstName: student.FirstName,
                LastName: student.LastName,
                XP: student.XP,
                Level: level,
                XpToNextLevel: 100 - xpIntoLevel,
                TotalSubmissions: student.Submissions.Count,
                CorrectSubmissions: student.Submissions.Count(s => s.IsCorrect),
                TotalAssignments: totalAssignments,
                CompletedAssignments: completedAssignments,
                CompletionRate: totalAssignments > 0
                    ? Math.Round((double)completedAssignments / totalAssignments * 100, 1)
                    : 0,
                Badges: student.Badges
                    .OrderByDescending(b => b.EarnedAt)
                    .Select(b => new BadgeDto(b.Name, b.Description, b.Icon, b.EarnedAt))
                    .ToList(),
                RecentSubmissions: student.Submissions
                    .OrderByDescending(s => s.SubmittedAt)
                    .Take(20)
                    .Select(s => new SubmissionHistoryDto(
                        Id: s.Id,
                        AssignmentTitle: s.Assignment?.Title ?? "Unknown",
                        Language: s.Assignment?.Language ?? "python",
                        IsCorrect: s.IsCorrect,
                        Output: s.Output,
                        SubmittedAt: s.SubmittedAt))
                    .ToList()
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetProgress failed for student {Id}", studentId);
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpGet("me")]
    public async Task<ActionResult<StudentProgressDto>> GetMyProgress()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.ApplicationUserId == userId);

            if (student is null)
                return NotFound(new { message = "Your account is not linked to a student record yet. Ask your tutor to link you." });

            return await GetProgress(student.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetMyProgress failed");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpGet("unlinked-users")]
    [Authorize(Roles = "Tutor")]
    public async Task<ActionResult<IEnumerable<UnlinkedUserDto>>> GetUnlinkedUsers()
    {
        try
        {
            var studentUsers = await _context.Users
                .Where(u => u.Role == UserRole.Student)
                .Select(u => new { u.Id, u.Email, u.FirstName, u.LastName })
                .ToListAsync();

            var linkedIds = await _context.Students
                .Where(s => s.ApplicationUserId != null)
                .Select(s => s.ApplicationUserId!)
                .ToListAsync();

            var result = studentUsers
                .Where(u => !linkedIds.Contains(u.Id))
                .Select(u => new UnlinkedUserDto(
                    Id: u.Id,
                    Email: u.Email ?? string.Empty,
                    FullName: $"{u.FirstName} {u.LastName}"))
                .ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetUnlinkedUsers failed");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPost("link/{studentId:int}")]
    [Authorize(Roles = "Tutor")]
    public async Task<IActionResult> LinkStudent(int studentId, [FromBody] LinkStudentDto dto)
    {
        try
        {
            var tutorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == studentId && s.TutorId == tutorId);

            if (student is null) return NotFound(new { message = "Student not found." });

            var alreadyLinked = await _context.Students
                .AnyAsync(s => s.ApplicationUserId == dto.ApplicationUserId);

            if (alreadyLinked)
                return BadRequest(new { message = "This user is already linked to another student record." });

            student.ApplicationUserId = dto.ApplicationUserId;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Linked successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LinkStudent failed");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpDelete("link/{studentId:int}")]
    [Authorize(Roles = "Tutor")]
    public async Task<IActionResult> UnlinkStudent(int studentId)
    {
        var tutorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.Id == studentId && s.TutorId == tutorId);

        if (student is null) return NotFound();
        student.ApplicationUserId = null;
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
