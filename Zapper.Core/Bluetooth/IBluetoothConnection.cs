using System.Threading.Tasks;

namespace Zapper.Core.Bluetooth;

public interface IBluetoothConnection
{
    Task Start();
    Task ScanForDevices();
    void Stop();
    event BluetoothConnection.RaiseBluetoothDeviceFound OnBluetoothDeviceFound;
}