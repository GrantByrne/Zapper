namespace Zapper.Contracts.Activities;

/// <summary>
/// Represents the response from executing an activity.
/// Contains the execution result and any relevant messages about the process.
/// </summary>
public class ExecuteActivityResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether the activity was executed successfully.
    /// False indicates that one or more steps failed during execution.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets a message describing the execution result.
    /// For successful executions, this may contain a summary.
    /// For failed executions, this contains error details.
    /// </summary>
    public string Message { get; set; } = "";
}