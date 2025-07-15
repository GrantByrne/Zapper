namespace Zapper.Contracts.Activities;

/// <summary>
/// Represents an activity that can be executed by the Zapper system.
/// Activities are sequences of device commands that can be triggered together,
/// such as "Watch TV" which might turn on the TV, switch to the correct input,
/// and turn on the sound system.
/// </summary>
public class ActivityDto
{
    /// <summary>
    /// Gets or sets the unique identifier for this activity.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the display name of the activity.
    /// This is shown to users in the interface.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Gets or sets an optional description of what this activity does.
    /// This provides additional context to users about the activity's purpose.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the type of activity (e.g., "Composite", "Simple").
    /// Defaults to "Composite" for activities that contain multiple steps.
    /// </summary>
    public string Type { get; set; } = "Composite";

    /// <summary>
    /// Gets or sets an optional URL to an icon image for this activity.
    /// The icon is displayed in the user interface to help identify the activity.
    /// </summary>
    public string? IconUrl { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this activity is enabled and can be executed.
    /// Disabled activities are hidden from the user interface.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Gets or sets the sort order for displaying this activity in lists.
    /// Lower numbers appear first in the user interface.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this activity was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this activity was last executed.
    /// This is used for analytics and sorting by recent usage.
    /// </summary>
    public DateTime LastUsed { get; set; }

    /// <summary>
    /// Gets or sets the collection of steps that make up this activity.
    /// Each step represents a command to be sent to a specific device.
    /// Steps are executed in the order specified by their SortOrder property.
    /// </summary>
    public ICollection<ActivityStepDto> Steps { get; set; } = new List<ActivityStepDto>();
}