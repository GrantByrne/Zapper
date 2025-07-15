namespace Zapper.Contracts;

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

/// <summary>
/// Represents a single step to be included when updating an activity.
/// Each step defines a command to be sent to a specific device with timing information.
/// </summary>
public class UpdateActivityStepRequest
{
    /// <summary>
    /// Gets or sets the unique identifier of the activity step.
    /// If null, this represents a new step to be created.
    /// If specified, this represents an existing step to be updated.
    /// </summary>
    public int? Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the device that should receive this command.
    /// This must reference a valid device configured in the system.
    /// </summary>
    public int DeviceId { get; set; }

    /// <summary>
    /// Gets or sets the command to be sent to the device.
    /// The format and content depend on the device type and connection method.
    /// </summary>
    public string Command { get; set; } = "";

    /// <summary>
    /// Gets or sets the delay in milliseconds to wait after executing this step.
    /// This allows time for devices to process commands before the next step is executed.
    /// Defaults to 500ms.
    /// </summary>
    public int DelayMs { get; set; } = 500;

    /// <summary>
    /// Gets or sets the sort order for this step within the activity.
    /// Steps are executed in ascending order of their SortOrder values.
    /// </summary>
    public int SortOrder { get; set; }
}