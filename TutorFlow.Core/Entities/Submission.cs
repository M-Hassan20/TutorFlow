namespace TutorFlow.Core.Entities;

public class Submission
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Output { get; set; }
    public bool IsCorrect { get; set; } = false;
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    // Foreign keys
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public int AssignmentId { get; set; }
    public Assignment Assignment { get; set; } = null!;
}
