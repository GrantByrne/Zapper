using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HashtagChris.DotNetBlueZ;
using HashtagChris.DotNetBlueZ.Extensions;
using Microsoft.Extensions.Logging;
using WebSocketSharp;

namespace Zapper.Core.Bluetooth;

public class BluetoothConnection : IDisposable, IBluetoothConnection
{
    private readonly ILogger<BluetoothConnection> _logger;
    
    private List<Adapter> _adapters = new();
    private bool _running;
    private readonly HashSet<(string, string)> _foundDevices = new();

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
                    await PublishDeviceDetails(device);
                }
            }
        }

        foreach (var a in _adapters)
        {
            await a.StopDiscoveryAsync();
        }
    }

    private async Task PublishDeviceDetails(Device device)
    {
        try
        {
            var properties = await device.GetAllAsync();

            (string, string) key = new(properties.Name, properties.Address);
            if (_foundDevices.Contains(key))
            {
                return;
            }

            _foundDevices.Add(key);

            var e = MapToEvent(properties);

            if (!e.Name.IsNullOrEmpty())
            {
                OnBluetoothDeviceFound?.Invoke(e);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while pulling back bluetooth device properties");
        }
    }

    private static BluetoothDeviceFoundEvent MapToEvent(Device1Properties properties)
    {
        var e = new BluetoothDeviceFoundEvent();

        e.Name = properties.Name;
        e.Address = properties.Address;
        e.Alias = properties.Alias;
        e.Icon = properties.Icon;
        e.Modalias = properties.Modalias;
        e.AddressType = properties.AddressType;
        
        return e;
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