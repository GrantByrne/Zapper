namespace Zapper.Client.UsbRemotes;

/// <summary>
/// Response from learning a button from a USB remote.
/// </summary>
public class LearnButtonResponse
{
    /// <summary>
    /// Gets or sets whether the button was learned successfully.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the key code of the learned button.
    /// </summary>
    public byte KeyCode { get; set; }

    /// <summary>
    /// Gets or sets the name of the learned button.
    /// </summary>
    public string ButtonName { get; set; } = "";

    /// <summary>
    /// Gets or sets a message describing the result.
    /// </summary>
    public string Message { get; set; } = "";
}