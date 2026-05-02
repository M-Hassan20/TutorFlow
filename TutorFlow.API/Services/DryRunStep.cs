namespace TutorFlow.API.Services;

public class DryRunStep
{
    public int Line { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Variable { get; set; }
    public string? Value { get; set; }
    public string? Output { get; set; }
    public string StepType { get; set; } = string.Empty; // "assignment" | "print" | "comment" | "unknown"
}
