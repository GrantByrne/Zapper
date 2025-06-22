using System.ComponentModel.DataAnnotations;

namespace Zapper.Core.Models;

public class Device
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Brand { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Model { get; set; } = string.Empty;
    
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
    
    public bool IsOnline { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
    
    public ICollection<DeviceCommand> Commands { get; set; } = new List<DeviceCommand>();
    
    public ICollection<ActivityDevice> ActivityDevices { get; set; } = new List<ActivityDevice>();
}