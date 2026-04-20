using Microsoft.EntityFrameworkCore;
using TutorFlow.Infrastructure;
using TutorFlow.Core.Entities;
using TutorFlow.Core.Interfaces;
using TutorFlow.Infrastructure.Data;

namespace TutorFlow.Infrastructure.Repositories;

public class AssignmentRepository : IAssignmentRepository
{
    private readonly AppDbContext _context;

    public AssignmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Assignment>> GetAllByTutorAsync(string tutorId)
    {
        return await _context.Assignments
            .Where(a => a.TutorId == tutorId)
            .Include(a => a.Students)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<Assignment?> GetByIdAsync(int id, string tutorId)
    {
        return await _context.Assignments
            .Include(a => a.Students)
            .Include(a => a.Submissions)
            .FirstOrDefaultAsync(a => a.Id == id && a.TutorId == tutorId);
    }

    public async Task<Assignment> CreateAsync(Assignment assignment)
    {
        _context.Assignments.Add(assignment);
        await _context.SaveChangesAsync();
        return assignment;
    }

    public async Task<Assignment?> UpdateAsync(Assignment assignment)
    {
        var existing = await _context.Assignments
            .FirstOrDefaultAsync(a => a.Id == assignment.Id && a.TutorId == assignment.TutorId);

        if (existing is null) return null;

        existing.Title = assignment.Title;
        existing.Description = assignment.Description;
        existing.StarterCode = assignment.StarterCode;
        existing.Language = assignment.Language;
        existing.XPReward = assignment.XPReward;
        existing.DueDate = assignment.DueDate;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id, string tutorId)
    {
        var assignment = await _context.Assignments
            .FirstOrDefaultAsync(a => a.Id == id && a.TutorId == tutorId);

        if (assignment is null) return false;

        _context.Assignments.Remove(assignment);
        await _context.SaveChangesAsync();
        return true;
    }
}
