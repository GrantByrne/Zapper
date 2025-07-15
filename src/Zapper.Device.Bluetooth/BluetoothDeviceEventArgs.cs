namespace Zapper.Device.Bluetooth;

public class BluetoothDeviceEventArgs : EventArgs
{
    public BluetoothDeviceInfo Device { get; }

    public BluetoothDeviceEventArgs(BluetoothDeviceInfo device)
    {
        Device = device ?? throw new ArgumentNullException(nameof(device));
    }
}