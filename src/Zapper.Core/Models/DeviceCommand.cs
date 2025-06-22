using System.ComponentModel.DataAnnotations;

namespace Zapper.Core.Models;

public class DeviceCommand
{
    public int Id { get; set; }
    
    public int DeviceId { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public CommandType Type { get; set; }
    
    public string? IrCode { get; set; }
    
    public string? NetworkPayload { get; set; }
    
    public string? HttpMethod { get; set; }
    
    public string? HttpEndpoint { get; set; }
    
    public int DelayMs { get; set; } = 0;
    
    public bool IsRepeatable { get; set; } = false;
    
    public int? MouseDeltaX { get; set; }
    
    public int? MouseDeltaY { get; set; }
    
    public string? KeyboardText { get; set; }
    
    public Device Device { get; set; } = null!;
    
    public ICollection<ActivityStep> ActivitySteps { get; set; } = new List<ActivityStep>();
}

public enum CommandType
{
    Power,
    VolumeUp,
    VolumeDown,
    Mute,
    ChannelUp,
    ChannelDown,
    Input,
    Menu,
    Back,
    Home,
    OK,
    DirectionalUp,
    DirectionalDown,
    DirectionalLeft,
    DirectionalRight,
    Number,
    PlayPause,
    Stop,
    FastForward,
    Rewind,
    Record,
    AppLaunch,
    Custom,
    MouseMove,
    MouseClick,
    KeyboardInput
}