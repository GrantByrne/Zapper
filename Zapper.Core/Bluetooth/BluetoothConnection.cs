using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HashtagChris.DotNetBlueZ;
using HashtagChris.DotNetBlueZ.Extensions;
using Microsoft.Extensions.Logging;

namespace Zapper.Core.Bluetooth;

public class BluetoothConnection : IDisposable, IBluetoothConnection
{
    private readonly ILogger<BluetoothConnection> _logger;
    
    private List<Adapter> _adapters = new();
    private bool _running;
    private HashSet<(string, string)> _foundDevices = new();

    public delegate void RaiseBluetoothDeviceFound(BluetoothDeviceFoundEvent e);

    public event RaiseBluetoothDeviceFound OnBluetoothDeviceFound;

    public BluetoothConnection(ILogger<BluetoothConnection> logger)
    {
        _logger = logger;
    }

    public async Task Start()
    {
        var adapters = await BlueZManager.GetAdaptersAsync();
        _adapters = adapters.ToList();
    }

    public async Task ScanForDevices()
    {
        _running = true;
        
        foreach (var a in _adapters)
        {
            await a.StartDiscoveryAsync();
        }

        while (_running)
        {
            foreach (var a in _adapters)
            { 
                var devices = await a.GetDevicesAsync();

                foreach (var device in devices)
                {
                    var properties = await device.GetAllAsync();

                    (string, string) key = new (properties.Name, properties.Address);
                    if (_foundDevices.Contains(key))
                    {
                        continue;
                    }

                    _foundDevices.Add(key);

                    var e = new BluetoothDeviceFoundEvent();
                    
                    e.Name = properties.Name;
                    e.Address = properties.Address;
                    e.Alias = properties.Alias;
                    e.Icon = properties.Icon;
                    e.Modalias = properties.Modalias;
                    e.AddressType = properties.AddressType;

                    OnBluetoothDeviceFound?.Invoke(e);
                }
            }
        }

        foreach (var a in _adapters)
        {
            await a.StopDiscoveryAsync();
        }
    }

    public void Stop()
    {
        _running = false;
    }

    public void Dispose()
    {
        Stop();
    }
}