namespace Zapper.Client.UsbRemotes;

/// <summary>
/// Request to enter learning mode for capturing a button press from a USB remote.
/// </summary>
public class LearnButtonRequest
{
    /// <summary>
    /// Gets or sets the ID of the USB remote.
    /// </summary>
    public int RemoteId { get; set; }

    /// <summary>
    /// Gets or sets the timeout in seconds to wait for a button press.
    /// Default value is 10 seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 10;
}