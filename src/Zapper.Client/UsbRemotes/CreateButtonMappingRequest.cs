using Zapper.Core.Models;

namespace Zapper.Client.UsbRemotes;

/// <summary>
/// Request to create a new button mapping for a USB remote button.
/// </summary>
public class CreateButtonMappingRequest
{
    /// <summary>
    /// Gets or sets the ID of the USB remote button.
    /// </summary>
    public int ButtonId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the device to control.
    /// </summary>
    public int DeviceId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the device command to execute.
    /// </summary>
    public int DeviceCommandId { get; set; }

    /// <summary>
    /// Gets or sets the button event type that triggers the command.
    /// </summary>
    public ButtonEventType EventType { get; set; }
}