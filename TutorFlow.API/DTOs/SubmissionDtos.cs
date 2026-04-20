namespace TutorFlow.API.DTOs;

public record CreateSubmissionDto(
    int StudentId,
    int AssignmentId,
    string Code
);

public record SubmissionResponseDto(
    int Id,
    int StudentId,
    string StudentName,
    int AssignmentId,
    string AssignmentTitle,
    string Code,
    string? Output,
    bool IsCorrect,
    DateTime SubmittedAt
);
