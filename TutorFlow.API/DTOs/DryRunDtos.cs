namespace TutorFlow.API.DTOs;

public record DryRunRequestDto(string Code);

public record DryRunStepDto(
    int Line,
    string Code,
    string? Variable,
    string? Value,
    string? Output,
    string StepType
);

public record DryRunResponseDto(
    List<DryRunStepDto> Steps,
    int TotalLines,
    int AssignmentCount,
    int PrintCount
);
