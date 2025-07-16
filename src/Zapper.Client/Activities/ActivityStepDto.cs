namespace Zapper.Client.Activities;

/// <summary>
/// Represents a single step within an activity sequence.
/// Each step defines a command to be sent to a specific device,
/// along with timing and ordering information.
/// </summary>
public class ActivityStepDto
{
    /// <summary>
    /// Gets or sets the unique identifier for this activity step.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the activity that contains this step.
    /// This establishes the parent-child relationship between activities and their steps.
    /// </summary>
    public int ActivityId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the device that should receive this command.
    /// This references a device configured in the system.
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
    /// </summary>
    public int DelayMs { get; set; }

    /// <summary>
    /// Gets or sets the sort order for this step within the activity.
    /// Steps are executed in ascending order of their SortOrder values.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Gets or sets the optional display name of the device for this step.
    /// This is used for display purposes and may be denormalized from the device table.
    /// </summary>
    public string? DeviceName { get; set; }
}