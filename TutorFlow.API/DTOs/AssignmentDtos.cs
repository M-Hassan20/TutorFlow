namespace TutorFlow.API.DTOs;

public record CreateAssignmentDto(
    string Title,
    string Description,
    string? StarterCode,
    string Language,
    int XPReward,
    DateTime? DueDate
);

public record UpdateAssignmentDto(
    string Title,
    string Description,
    string? StarterCode,
    string Language,
    int XPReward,
    DateTime? DueDate
);

public record AssignmentResponseDto(
    int Id,
    string Title,
    string Description,
    string? StarterCode,
    string Language,
    int XPReward,
    DateTime CreatedAt,
    DateTime? DueDate,
    int StudentCount
);
