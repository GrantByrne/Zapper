namespace Zapper.Client.Devices;

public class BluetoothControlRequest
{
    public string Action { get; set; } = "";
    public string? DeviceId { get; set; }
    public string? KeyCode { get; set; }
    public string? Text { get; set; }
    public int? MouseX { get; set; }
    public int? MouseY { get; set; }
    public bool? LeftClick { get; set; }
    public bool? RightClick { get; set; }
}