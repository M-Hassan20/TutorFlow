using TutorFlow.Core.Entities;

namespace TutorFlow.Core.Interfaces;

public interface IAssignmentRepository
{
    Task<IEnumerable<Assignment>> GetAllByTutorAsync(string tutorId);
    Task<Assignment?> GetByIdAsync(int id, string tutorId);
    Task<Assignment> CreateAsync(Assignment assignment);
    Task<Assignment?> UpdateAsync(Assignment assignment);
    Task<bool> DeleteAsync(int id, string tutorId);
}
