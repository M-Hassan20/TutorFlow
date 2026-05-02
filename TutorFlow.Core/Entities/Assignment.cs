namespace TutorFlow.Core.Entities;

public class Assignment
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? StarterCode { get; set; }
    public string Language { get; set; } = "python"; // default language
    public int XPReward { get; set; } = 50;
    public string? ExpectedOutput { get; set; } //To grade submissions
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }

    // Foreign key — which tutor created this
    public string TutorId { get; set; } = string.Empty;
    //public ApplicationUser Tutor { get; set; } = null!;

    // Navigation
    public ICollection<Student> Students { get; set; } = new List<Student>();
    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
