namespace Zapper.Client;

/// <summary>
/// Represents a request to create a new activity in the system.
/// Activities are sequences of device commands that can be executed together.
/// </summary>
public class CreateActivityRequest
{
    /// <summary>
    /// Gets or sets the display name for the new activity.
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
    /// Gets or sets a value indicating whether the activity should be enabled when created.
    /// Disabled activities are hidden from the user interface.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the list of steps that make up this activity.
    /// Each step represents a command to be sent to a specific device.
    /// </summary>
    public List<CreateActivityStepRequest> Steps { get; set; } = [];
}