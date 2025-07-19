namespace Zapper.Services;

public class BluetoothHost
{
    public string Address { get; set; } = string.Empty;
    public string? Name { get; set; }
    public DateTime LastConnected { get; set; }
    public bool IsPaired { get; set; }
}