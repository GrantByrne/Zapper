namespace Zapper.Device.Bluetooth;

public class BluetoothDeviceEventArgs(BluetoothDeviceInfo device) : EventArgs
{
    public BluetoothDeviceInfo Device { get; } = device ?? throw new ArgumentNullException(nameof(device));
}