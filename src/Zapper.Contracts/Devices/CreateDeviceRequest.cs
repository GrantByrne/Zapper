namespace Zapper.Contracts.Devices;

/// <summary>
/// Represents a request to create a new device in the system.
/// Contains all the information needed to configure a device for control by the Zapper system.
/// </summary>
public class CreateDeviceRequest
{
    /// <summary>
    /// Gets or sets the display name for the new device.
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

    /// <summary>
    /// Gets or sets the IR code set identifier for infrared-controlled devices.
    /// This references the specific set of IR codes that work with this device model.
    /// </summary>
    public int? IrCodeSetId { get; set; }
}