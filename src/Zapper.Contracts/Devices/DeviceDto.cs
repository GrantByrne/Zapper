using System.ComponentModel.DataAnnotations;

namespace Zapper.Contracts.Devices;

/// <summary>
/// Represents a device that can be controlled by the Zapper system.
/// Devices include televisions, receivers, streaming devices, and other home entertainment equipment
/// that can be controlled via IR, Bluetooth, network protocols, or other connection methods.
/// </summary>
public class DeviceDto
{
    /// <summary>
    /// Gets or sets the unique identifier for this device.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the display name of the device.
    /// This is shown to users in the interface and should be descriptive (e.g., "Living Room TV").
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = "";

    /// <summary>
    /// Gets or sets the brand or manufacturer of the device (e.g., "Samsung", "LG", "Sony").
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Brand { get; set; } = "";

    /// <summary>
    /// Gets or sets the model number or name of the device.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Model { get; set; } = "";

    /// <summary>
    /// Gets or sets the type of device (e.g., Television, Receiver, StreamingDevice).
    /// This determines the default commands available and UI presentation.
    /// </summary>
    [Required]
    public DeviceType Type { get; set; }

    /// <summary>
    /// Gets or sets the connection method used to communicate with this device.
    /// Supported types include IR, Bluetooth, network protocols, and USB.
    /// </summary>
    [Required]
    public ConnectionType ConnectionType { get; set; }

    /// <summary>
    /// Gets or sets the IP address for network-connected devices.
    /// Required for devices using network-based connection types.
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Gets or sets the MAC address of the device for network identification.
    /// Used for device discovery and Wake-on-LAN functionality.
    /// </summary>
    public string? MacAddress { get; set; }

    /// <summary>
    /// Gets or sets the network port used for communication with the device.
    /// Different devices and protocols use different default ports.
    /// </summary>
    public int? Port { get; set; }

    /// <summary>
    /// Gets or sets the authentication token for the device.
    /// This is an alias for AuthenticationToken and may be deprecated.
    /// </summary>
    public string? AuthToken { get; set; }

    /// <summary>
    /// Gets or sets the network address for the device.
    /// This may be used as an alternative to IP address for some connection types.
    /// </summary>
    public string? NetworkAddress { get; set; }

    /// <summary>
    /// Gets or sets the authentication token required to communicate with the device.
    /// Many smart devices require authentication tokens for security.
    /// </summary>
    public string? AuthenticationToken { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use a secure connection (HTTPS/TLS) when communicating with the device.
    /// </summary>
    public bool UseSecureConnection { get; set; }

    /// <summary>
    /// Gets or sets the Bluetooth address (MAC address) for Bluetooth-connected devices.
    /// </summary>
    public string? BluetoothAddress { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the device supports mouse input commands.
    /// This is relevant for devices like Android TV or Apple TV that support cursor control.
    /// </summary>
    public bool SupportsMouseInput { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the device supports keyboard input commands.
    /// This is relevant for devices that can receive text input.
    /// </summary>
    public bool SupportsKeyboardInput { get; set; }

    /// <summary>
    /// Gets or sets the IR code set identifier for infrared-controlled devices.
    /// This references the specific set of IR codes that work with this device model.
    /// </summary>
    public string? IrCodeSet { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the device is currently online and reachable.
    /// This is updated by the system when attempting to communicate with the device.
    /// </summary>
    public bool IsOnline { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this device was created in the system.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the device was last seen or successfully communicated with.
    /// This is used to determine device availability and troubleshoot connection issues.
    /// </summary>
    public DateTime LastSeen { get; set; }
}