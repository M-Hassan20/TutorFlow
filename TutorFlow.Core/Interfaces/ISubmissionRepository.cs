using TutorFlow.Core.Entities;

namespace TutorFlow.Core.Interfaces;

public interface ISubmissionRepository
{
    Task<IEnumerable<Submission>> GetByStudentAsync(int studentId);
    Task<IEnumerable<Submission>> GetByAssignmentAsync(int assignmentId);
    Task<Submission> CreateAsync(Submission submission);
}
