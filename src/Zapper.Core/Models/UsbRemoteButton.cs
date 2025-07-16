using System.ComponentModel.DataAnnotations;

namespace Zapper.Core.Models;

public class UsbRemoteButton
{
    public int Id { get; set; }

    [Required]
    public int RemoteId { get; set; }

    [Required]
    public byte KeyCode { get; set; }

    [Required]
    [MaxLength(50)]
    public string ButtonName { get; set; } = "";

    [MaxLength(200)]
    public string? Description { get; set; }

    public bool IsSystemButton { get; set; } = false;

    public bool AllowInterception { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public UsbRemote Remote { get; set; } = null!;

    public ICollection<UsbRemoteButtonMapping> Mappings { get; set; } = new List<UsbRemoteButtonMapping>();
}