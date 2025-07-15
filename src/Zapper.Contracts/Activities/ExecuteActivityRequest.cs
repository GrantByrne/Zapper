namespace Zapper.Contracts.Activities;

/// <summary>
/// Represents a request to execute an activity by its identifier.
/// When executed, the activity will run all of its configured steps in sequence.
/// </summary>
public class ExecuteActivityRequest
{
    /// <summary>
    /// Gets or sets the unique identifier of the activity to execute.
    /// </summary>
    public int Id { get; set; }
}