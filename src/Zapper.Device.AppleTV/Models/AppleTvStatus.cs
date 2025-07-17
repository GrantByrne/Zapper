namespace Zapper.Device.AppleTV.Models;

public class AppleTvStatus
{
    public bool IsPlaying { get; set; }
    public string? CurrentApp { get; set; }
    public string? Title { get; set; }
    public string? Artist { get; set; }
    public string? Album { get; set; }
    public TimeSpan? Duration { get; set; }
    public TimeSpan? Position { get; set; }
    public int Volume { get; set; }
    public bool IsMuted { get; set; }
    public byte[]? Artwork { get; set; }
    public PlaybackState PlaybackState { get; set; }
}

public enum PlaybackState
{
    Unknown,
    Playing,
    Paused,
    Stopped,
    FastForward,
    Rewind
}