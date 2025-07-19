namespace Zapper.Client;

/// <summary>
/// Represents a request to update an existing activity in the system.
/// This allows modification of activity properties and the complete replacement of activity steps.
/// </summary>
public class UpdateActivityRequest
{
    /// <summary>
    /// Gets or sets the unique identifier of the activity to update.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the display name for the activity.
    /// This will be shown to users in the interface.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Gets or sets an optional description of what the activity does.
    /// This provides additional context to users about the activity's purpose.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the type of activity (e.g., "Composite", "Simple").
    /// Defaults to "Composite" for activities that contain multiple steps.
    /// </summary>
    public string Type { get; set; } = "Composite";

    /// <summary>
    /// Gets or sets a value indicating whether the activity is enabled and can be executed.
    /// Disabled activities are hidden from the user interface.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the list of steps that make up this activity.
    /// This will completely replace the existing steps for the activity.
    /// </summary>
    public List<UpdateActivityStepRequest> Steps { get; set; } = [];
}