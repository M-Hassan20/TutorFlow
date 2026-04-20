using TutorFlow.Core.Entities;

namespace TutorFlow.Core.Interfaces;

public interface IStudentRepository
{
    Task<IEnumerable<Student>> GetAllByTutorAsync(string tutorId);
    Task<Student?> GetByIdAsync(int id, string tutorId);
    Task<Student> CreateAsync(Student student);
    Task<Student?> UpdateAsync(Student student);
    Task<bool> DeleteAsync(int id, string tutorId);
}
