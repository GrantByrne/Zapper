using System.ComponentModel.DataAnnotations;

namespace Zapper.Core.Models;

public class Device
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = "";

    [Required]
    [MaxLength(50)]
    public string Brand { get; set; } = "";

    [Required]
    [MaxLength(50)]
    public string Model { get; set; } = "";

    [Required]
    public DeviceType Type { get; set; }

    [Required]
    public ConnectionType ConnectionType { get; set; }

    public string? IpAddress { get; set; }

    public string? MacAddress { get; set; }

    public int? Port { get; set; }

    public string? AuthToken { get; set; }

    public string? NetworkAddress { get; set; }

    public string? AuthenticationToken { get; set; }

    public bool UseSecureConnection { get; set; }

    public string? BluetoothAddress { get; set; }

    public bool SupportsMouseInput { get; set; }

    public bool SupportsKeyboardInput { get; set; }

    public string? IrCodeSet { get; set; }

    public int? IrCodeSetId { get; set; }

    public bool IsOnline { get; set; }

    public string? DeviceIdentifier { get; set; }

    public string? ProtocolVersion { get; set; }

    public bool RequiresPairing { get; set; }

    public bool IsPaired { get; set; }

    public string? PairingId { get; set; }

    public byte[]? PairingKey { get; set; }

    public string? SessionId { get; set; }

    public Dictionary<string, string>? ServiceProperties { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime LastSeen { get; set; } = DateTime.UtcNow;

    public ICollection<DeviceCommand> Commands { get; set; } = new List<DeviceCommand>();

    public ICollection<ActivityDevice> ActivityDevices { get; set; } = new List<ActivityDevice>();
}