namespace TutorFlow.API.DTOs;

public record RegisterDto(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string Role  // "Tutor" | "Student" | "Parent"
);

public record LoginDto(
    string Email,
    string Password
);

public record AuthResponseDto(
    string Token,
    string Email,
    string FullName,
    string Role,
    DateTime Expiry
);
