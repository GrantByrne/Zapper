namespace Zapper.Device.Bluetooth;

public class BluetoothDeviceInfo
{
    public string Address { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Alias { get; set; }
    public bool IsConnected { get; set; }
    public bool IsPaired { get; set; }
    public bool IsTrusted { get; set; }
    public bool IsBlocked { get; set; }
    public short? Rssi { get; set; }
    public short? TxPower { get; set; }
    public uint? Class { get; set; }
    public string[] UuiDs { get; set; } = [];
}