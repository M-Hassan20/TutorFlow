using Microsoft.EntityFrameworkCore;
using TutorFlow.Core.Entities;
using TutorFlow.Infrastructure.Data;

namespace TutorFlow.API.Services;

public class BadgeService
{
    private readonly AppDbContext _context;

    public BadgeService(AppDbContext context)
    {
        _context = context;
    }

    public async Task AwardBadgesAsync(int studentId)
    {
        var student = await _context.Students
            .Include(s => s.Submissions)
            .Include(s => s.Badges)
            .Include(s => s.Assignments)
            .FirstOrDefaultAsync(s => s.Id == studentId);

        if (student is null) return;

        var existingBadgeNames = student.Badges.Select(b => b.Name).ToHashSet();
        var newBadges = new List<Badge>();

        void TryAward(string name, string desc, string icon)
        {
            if (!existingBadgeNames.Contains(name))
                newBadges.Add(new Badge { Name = name, Description = desc, Icon = icon, StudentId = studentId });
        }

        var totalSubmissions = student.Submissions.Count;
        var correctSubmissions = student.Submissions.Count(s => s.IsCorrect);
        var completedAssignments = student.Submissions
            .Where(s => s.IsCorrect)
            .Select(s => s.AssignmentId)
            .Distinct()
            .Count();

        // ── Badge definitions ─────────────────────────────────────────────
        if (totalSubmissions >= 1)
            TryAward("First Steps", "Submitted your first piece of code", "🚀");

        if (correctSubmissions >= 1)
            TryAward("Problem Solver", "Got your first correct answer", "✅");

        if (totalSubmissions >= 5)
            TryAward("Persistent", "Made 5 code submissions", "💪");

        if (totalSubmissions >= 10)
            TryAward("Dedicated", "Made 10 code submissions", "🔥");

        if (correctSubmissions >= 5)
            TryAward("Sharp Mind", "Got 5 correct answers", "🧠");

        if (student.XP >= 100)
            TryAward("Century Club", "Earned 100 XP", "⭐");

        if (student.XP >= 500)
            TryAward("XP Machine", "Earned 500 XP", "💎");

        if (student.Assignments.Count > 0 &&
            completedAssignments >= student.Assignments.Count)
            TryAward("Completionist", "Completed all assigned work", "🏆");

        if (newBadges.Count > 0)
        {
            _context.Badges.AddRange(newBadges);
            await _context.SaveChangesAsync();
        }
    }
}
