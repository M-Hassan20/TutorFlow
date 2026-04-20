using Microsoft.EntityFrameworkCore;
using TutorFlow.Infrastructure;
using TutorFlow.Core.Entities;
using TutorFlow.Core.Interfaces;
using TutorFlow.Infrastructure.Data;

namespace TutorFlow.Infrastructure.Repositories;

public class SubmissionRepository : ISubmissionRepository
{
    private readonly AppDbContext _context;

    public SubmissionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Submission>> GetByStudentAsync(int studentId)
    {
        return await _context.Submissions
            .Where(s => s.StudentId == studentId)
            .Include(s => s.Assignment)
            .OrderByDescending(s => s.SubmittedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Submission>> GetByAssignmentAsync(int assignmentId)
    {
        return await _context.Submissions
            .Where(s => s.AssignmentId == assignmentId)
            .Include(s => s.Student)
            .OrderByDescending(s => s.SubmittedAt)
            .ToListAsync();
    }

    public async Task<Submission> CreateAsync(Submission submission)
    {
        _context.Submissions.Add(submission);

        // Award XP to student on first correct submission
        if (submission.IsCorrect)
        {
            var student = await _context.Students.FindAsync(submission.StudentId);
            var assignment = await _context.Assignments.FindAsync(submission.AssignmentId);
            if (student != null && assignment != null)
            {
                var alreadyAwarded = await _context.Submissions
                    .AnyAsync(s => s.StudentId == submission.StudentId
                               && s.AssignmentId == submission.AssignmentId
                               && s.IsCorrect);

                if (!alreadyAwarded)
                    student.XP += assignment.XPReward;
            }
        }

        await _context.SaveChangesAsync();
        return submission;
    }
}
