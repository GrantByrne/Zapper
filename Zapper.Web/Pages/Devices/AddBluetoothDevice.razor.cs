using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Zapper.Core.Bluetooth;

namespace Zapper.Web.Pages.Devices;

public partial class AddBluetoothDevice
{
    private readonly List<BluetoothDeviceFoundEvent> _devices = new();
    private BluetoothDeviceFoundEvent _selectedDevice;

    private async Task StartScanning()
    {
        try
        {
            _bluetoothConnection.OnBluetoothDeviceFound += FoundBluetoothDevice;
            await _bluetoothConnection.Start();
            await _bluetoothConnection.ScanForDevices();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while scanning for bluetooth devices");
        }
    }

    private void StopScanning()
    {
        try
        {
            _bluetoothConnection.OnBluetoothDeviceFound -= FoundBluetoothDevice;
            _bluetoothConnection.Stop();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while stopping the scan of bluetooth devices");
        }
    }

    private void FoundBluetoothDevice(BluetoothDeviceFoundEvent e)
    {
        try
        {
            _devices.Add(e);
            StateHasChanged();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred when processing found bluetooth device event");
        }
    }

    private async Task Connect()
    {
        try
        {
            _logger.LogInformation("Attempting to connect to bluetooth device");

            await _bluetoothConnection.Connect(_selectedDevice.Device);
            
            _logger.LogInformation("Finished connecting to bluetooth device");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to bluetooth device.");
        }
    }
}