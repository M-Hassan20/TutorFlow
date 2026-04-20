namespace TutorFlow.API.DTOs;

public record CreateStudentDto(
    string FirstName,
    string LastName,
    int Age,
    string? ParentEmail
);

public record UpdateStudentDto(
    string FirstName,
    string LastName,
    int Age,
    string? ParentEmail
);

public record StudentResponseDto(
    int Id,
    string FirstName,
    string LastName,
    int Age,
    string? ParentEmail,
    int XP,
    DateTime EnrolledAt,
    bool IsActive
);
