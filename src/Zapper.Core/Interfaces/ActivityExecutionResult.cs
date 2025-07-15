namespace Zapper.Core.Interfaces;

public class ActivityExecutionResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public TimeSpan ExecutionTime { get; set; }
    public List<StepResult> StepResults { get; set; } = new();
}