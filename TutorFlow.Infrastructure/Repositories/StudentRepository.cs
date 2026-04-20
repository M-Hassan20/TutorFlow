using Microsoft.EntityFrameworkCore;
using TutorFlow.Infrastructure;
using TutorFlow.Core.Entities;
using TutorFlow.Core.Interfaces;
using TutorFlow.Infrastructure.Data;

namespace TutorFlow.Infrastructure.Repositories;

public class StudentRepository : IStudentRepository
{
    private readonly AppDbContext _context;

    public StudentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Student>> GetAllByTutorAsync(string tutorId)
    {
        return await _context.Students
            .Where(s => s.TutorId == tutorId && s.IsActive)
            .OrderBy(s => s.FirstName)
            .ToListAsync();
    }

    public async Task<Student?> GetByIdAsync(int id, string tutorId)
    {
        return await _context.Students
            .Include(s => s.Assignments)
            .Include(s => s.Submissions)
            .FirstOrDefaultAsync(s => s.Id == id && s.TutorId == tutorId);
    }

    public async Task<Student> CreateAsync(Student student)
    {
        _context.Students.Add(student);
        await _context.SaveChangesAsync();
        return student;
    }

    public async Task<Student?> UpdateAsync(Student student)
    {
        var existing = await _context.Students
            .FirstOrDefaultAsync(s => s.Id == student.Id && s.TutorId == student.TutorId);

        if (existing is null) return null;

        existing.FirstName = student.FirstName;
        existing.LastName = student.LastName;
        existing.Age = student.Age;
        existing.ParentEmail = student.ParentEmail;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id, string tutorId)
    {
        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.Id == id && s.TutorId == tutorId);

        if (student is null) return false;

        // Soft delete
        student.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }
}
