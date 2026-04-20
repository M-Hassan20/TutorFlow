namespace TutorFlow.Core.Entities;

public class Student
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string? ParentEmail { get; set; }
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // XP / Gamification (used in Phase 4 but defined now)
    public int XP { get; set; } = 0;

    // Foreign key — which tutor owns this student
    public string TutorId { get; set; } = string.Empty;
    //public ApplicationUser Tutor { get; set; } = null!;

    // Navigation
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
