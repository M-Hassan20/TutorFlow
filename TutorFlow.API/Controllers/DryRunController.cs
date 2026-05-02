using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TutorFlow.API.DTOs;
using TutorFlow.API.Services;

namespace TutorFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DryRunController : ControllerBase
{
    private readonly DryRunService _dryRun;

    public DryRunController(DryRunService dryRun)
    {
        _dryRun = dryRun;
    }

    /// <summary>
    /// Simulate Python code step-by-step and return a variable/output trace table.
    /// </summary>
    [HttpPost]
    public ActionResult<DryRunResponseDto> Simulate([FromBody] DryRunRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Code))
            return BadRequest(new { message = "Code cannot be empty." });

        var steps = _dryRun.Simulate(dto.Code);

        var stepDtos = steps.Select(s => new DryRunStepDto(
            Line: s.Line,
            Code: s.Code,
            Variable: s.Variable,
            Value: s.Value,
            Output: s.Output,
            StepType: s.StepType
        )).ToList();

        return Ok(new DryRunResponseDto(
            Steps: stepDtos,
            TotalLines: steps.Count,
            AssignmentCount: steps.Count(s => s.StepType == "assignment"),
            PrintCount: steps.Count(s => s.StepType == "print")
        ));
    }
}
