namespace Zapper.Client.Devices;

/// <summary>
/// Represents a discovered PlayStation device
/// </summary>
public class PlayStationDeviceDto
{
    /// <summary>
    /// Name of the PlayStation device
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// IP address of the PlayStation device
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// Model of the PlayStation (e.g., "PS4", "PS5")
    /// </summary>
    public string? Model { get; set; }
}