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

    // XP / Gamification
    public int XP { get; set; } = 0;

    // Link to identity user — set when tutor links a registered student
    public string? ApplicationUserId { get; set; }

    // Foreign key — which tutor owns this student
    public string TutorId { get; set; } = string.Empty;

    // Navigation
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    public ICollection<Badge> Badges { get; set; } = new List<Badge>();
}
