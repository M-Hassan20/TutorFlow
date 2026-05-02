namespace TutorFlow.Core.Entities;

public class Badge
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty; // emoji or icon key
    public DateTime EarnedAt { get; set; } = DateTime.UtcNow;

    // Foreign key
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;
}
