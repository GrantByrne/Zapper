namespace Zapper.Client.Devices;

/// <summary>
/// Represents a request to send a command to a specific device.
/// Commands can include optional parameters depending on the device type and command.
/// </summary>
public class SendCommandRequest
{
    /// <summary>
    /// Gets or sets the command to be sent to the device.
    /// The format and content depend on the device type and connection method.
    /// Common examples include "power_on", "volume_up", "channel_2", etc.
    /// </summary>
    public string Command { get; set; } = "";

    /// <summary>
    /// Gets or sets optional parameters for the command.
    /// Different commands may require different parameters.
    /// For example, a "set_volume" command might include a "level" parameter.
    /// </summary>
    public Dictionary<string, object>? Parameters { get; set; }

    /// <summary>
    /// Gets or sets the horizontal mouse movement delta for mouse move commands.
    /// </summary>
    public int? MouseDeltaX { get; set; }

    /// <summary>
    /// Gets or sets the vertical mouse movement delta for mouse move commands.
    /// </summary>
    public int? MouseDeltaY { get; set; }

    /// <summary>
    /// Gets or sets the text to send for keyboard input commands.
    /// </summary>
    public string? KeyboardText { get; set; }
}