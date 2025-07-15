namespace Zapper.Core.Interfaces;

public class StepResult
{
    public int StepId { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public TimeSpan ExecutionTime { get; set; }
}