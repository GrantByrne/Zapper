namespace Zapper.Core.Bluetooth;

public class BluetoothDeviceFoundEvent
{
    public string Name { get; set; }
    public string Address { get; set; }
    public string Alias { get; set; }
    public string Icon { get; set; }
    public string Modalias{ get; set; }
    public string AddressType { get; set; }
}