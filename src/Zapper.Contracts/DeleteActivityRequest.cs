namespace Zapper.Contracts;

/// <summary>
/// Represents a request to delete an activity from the system.
/// This will permanently remove the activity and all of its associated steps.
/// </summary>
public class DeleteActivityRequest
{
    /// <summary>
    /// Gets or sets the unique identifier of the activity to delete.
    /// </summary>
    public int Id { get; set; }
}