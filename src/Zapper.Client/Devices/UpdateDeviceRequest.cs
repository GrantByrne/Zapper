using Zapper.Core.Models;

namespace Zapper.Client.Devices;

/// <summary>
/// Represents a request to update an existing device in the system.
/// Contains all the modifiable properties of a device configuration.
/// </summary>
public class UpdateDeviceRequest
{
    /// <summary>
    /// Gets or sets the ID of the device to update.
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Gets or sets the display name for the device.
    /// This should be descriptive (e.g., "Living Room TV") and will be shown to users.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Gets or sets the brand or manufacturer of the device (e.g., "Samsung", "LG", "Sony").
    /// This is optional but helps with device identification and command lookup.
    /// </summary>
    public string? Brand { get; set; }

    /// <summary>
    /// Gets or sets the model number or name of the device.
    /// This is optional but helps with device identification and command lookup.
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Gets or sets the type of device (e.g., Television, Receiver, StreamingDevice).
    /// This determines the default commands available and UI presentation.
    /// </summary>
    public DeviceType Type { get; set; }

    /// <summary>
    /// Gets or sets the connection method used to communicate with this device.
    /// Supported types include IR, Bluetooth, network protocols, and USB.
    /// </summary>
    public ConnectionType ConnectionType { get; set; }

    /// <summary>
    /// Gets or sets the IP address for network-connected devices.
    /// Required for devices using network-based connection types.
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Gets or sets the network port used for communication with the device.
    /// Different devices and protocols use different default ports.
    /// </summary>
    public int? Port { get; set; }

    /// <summary>
    /// Gets or sets the MAC address of the device for network identification.
    /// Used for device discovery and Wake-on-LAN functionality.
    /// </summary>
    public string? MacAddress { get; set; }

    /// <summary>
    /// Gets or sets the authentication token required to communicate with the device.
    /// Many smart devices require authentication tokens for security.
    /// </summary>
    public string? AuthenticationToken { get; set; }
}