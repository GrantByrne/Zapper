namespace Zapper.Client;

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