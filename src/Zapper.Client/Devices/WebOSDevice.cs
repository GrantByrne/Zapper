namespace Zapper.Client.Devices;

/// <summary>
/// Represents a WebOS device discovered during network scanning.
/// WebOS devices are typically LG smart TVs that can be controlled via WebSocket connections.
/// </summary>
public class WebOsDevice
{
    /// <summary>
    /// Gets or sets the friendly name of the WebOS device.
    /// This is typically the device name configured by the user.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Gets or sets the IP address of the WebOS device on the network.
    /// This is used to establish WebSocket connections for control.
    /// </summary>
    public string IpAddress { get; set; } = "";

    /// <summary>
    /// Gets or sets the model name of the WebOS device.
    /// This is optional and may not be available for all devices.
    /// </summary>
    public string? ModelName { get; set; }

    /// <summary>
    /// Gets or sets the model number of the WebOS device.
    /// This is optional and may not be available for all devices.
    /// </summary>
    public string? ModelNumber { get; set; }

    /// <summary>
    /// Gets or sets the port number for WebSocket connections.
    /// Defaults to 3000, which is the standard WebOS port.
    /// </summary>
    public int Port { get; set; } = 3000;
}