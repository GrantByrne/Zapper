using System.ComponentModel.DataAnnotations;

namespace Zapper.Core.Models;

public class UsbRemoteButtonMapping
{
    public int Id { get; set; }

    [Required]
    public int ButtonId { get; set; }

    [Required]
    public int DeviceId { get; set; }

    [Required]
    public int DeviceCommandId { get; set; }

    [Required]
    public ButtonEventType EventType { get; set; } = ButtonEventType.KeyPress;

    public int Priority { get; set; } = 0;

    public bool IsEnabled { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public UsbRemoteButton Button { get; set; } = null!;

    public Device Device { get; set; } = null!;

    public DeviceCommand DeviceCommand { get; set; } = null!;
}