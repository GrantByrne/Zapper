using System.Threading.Tasks;
using HashtagChris.DotNetBlueZ;

namespace Zapper.Core.Bluetooth;

public interface IBluetoothConnection
{
    Task Start();
    
    Task ScanForDevices();
    
    void Stop();
    
    event BluetoothConnection.RaiseBluetoothDeviceFound OnBluetoothDeviceFound;
    
    Task Connect(Device device);
}