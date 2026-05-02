namespace TutorFlow.API.DTOs;

public record BadgeDto(
    string Name,
    string Description,
    string Icon,
    DateTime EarnedAt
);

public record SubmissionHistoryDto(
    int Id,
    string AssignmentTitle,
    string Language,
    bool IsCorrect,
    string? Output,
    DateTime SubmittedAt
);

public record StudentProgressDto(
    int StudentId,
    string FirstName,
    string LastName,
    int XP,
    int Level,
    int XpToNextLevel,
    int TotalSubmissions,
    int CorrectSubmissions,
    int TotalAssignments,
    int CompletedAssignments,
    double CompletionRate,
    List<BadgeDto> Badges,
    List<SubmissionHistoryDto> RecentSubmissions
);

public record LinkStudentDto(string ApplicationUserId);

public record UnlinkedUserDto(string Id, string Email, string FullName);
