using System.Diagnostics;

namespace TutorFlow.API.Services;

public class CodeExecutionResult
{
    public string Output { get; set; } = string.Empty;
    public string? Error { get; set; }
    public bool HasError => !string.IsNullOrWhiteSpace(Error);
}

public class PistonService
{
    private readonly ILogger<PistonService> _logger;

    public PistonService(ILogger<PistonService> logger)
    {
        _logger = logger;
    }

    public async Task<CodeExecutionResult> ExecuteAsync(string language, string code)
    {
        if (language.ToLower() != "python")
            return new CodeExecutionResult { Error = $"Language '{language}' not supported in local mode." };

        // Write code to a temp file
        var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.py");
        try
        {
            await File.WriteAllTextAsync(tempFile, code);

            var psi = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = $"\"{tempFile}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = psi };
            process.Start();

            // Read output with a 10 second timeout
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();
            var exitTask = Task.Run(() => process.WaitForExit(10000));

            await Task.WhenAll(outputTask, errorTask, exitTask);

            var stdout = outputTask.Result.Trim();
            var stderr = errorTask.Result.Trim();

            if (!exitTask.Result)
            {
                try { process.Kill(); } catch { /* ignore kill failures */ }
                return new CodeExecutionResult { Error = "Execution timed out (10s limit)." };
            }

            return new CodeExecutionResult
            {
                Output = stdout,
                Error = string.IsNullOrWhiteSpace(stderr) ? null : stderr
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Code execution failed");
            return new CodeExecutionResult { Error = $"Execution failed: {ex.Message}. Is Python installed?" };
        }
        finally
        {
            // Always clean up the temp file
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }
}