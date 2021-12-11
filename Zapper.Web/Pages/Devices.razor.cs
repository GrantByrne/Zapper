using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Zapper.Core.Devices;
using Zapper.Web.Data;
using Zapper.Web.Shared;

namespace Zapper.Web.Pages;

public partial class Devices
{
    private List<DeviceModel> _devices = new();
    private Modal Modal { get; set; }
    private ConfigureWebOsDevice ConfigureWebOsDevice { get; set; }
    private ConfigureIrDevice ConfigureIrDevice { get; set; }
    private readonly string[] _supportedDeviceTypes = SupportedDevice.All();

    private string _selectedDeviceType;
    private bool _showSaveChanges;

    protected override void OnInitialized()
    {
        var devices = _deviceManager.Get();

        foreach (var device in devices)
        {
            var d = new DeviceModel();

            d.Id = device.Id;
            d.Status = _webOsStatusManager.GetStatus(d.Id);
            d.DeviceType = device.SupportDeviceType;
            d.Name = device.Name;

            _devices.Add(d);
        }
    }

    private void AddDevice()
    {
        try
        {
            _logger.LogInformation("Bring up the modal to add a new device");
            Modal.Open();
            _logger.LogInformation("Brought up the modal to add a new device");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to bring up the modal for a new device");
        }
    }

    private void SelectDeviceType(string deviceType)
    {
        try
        {
            _logger.LogInformation($"Selected the {deviceType} device type");
            _selectedDeviceType = deviceType;
            _showSaveChanges = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while selecting device type");
        }
    }

    private void SaveChanges()
    {
        try
        {
            _logger.LogInformation("Attempting to save changes");

            switch (_selectedDeviceType)
            {
                case SupportedDevice.WebOs:
                    var webOsName = ConfigureWebOsDevice.Name;
                    var ipAddress = ConfigureWebOsDevice.IpAddress;
                    var macAddress = ConfigureWebOsDevice.MacAddress;
                    _deviceManager.CreateWebOsDevice(webOsName, ipAddress, macAddress);
                    break;
                case SupportedDevice.Ir:
                    var irName = ConfigureIrDevice.Name;
                    _deviceManager.CreateIrDevice(irName);
                    break;
            }

            Modal.Close();

            _logger.LogInformation("Finished saving changes");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save changes");
        }
    }

    private void Delete(DeviceModel device)
    {
        try
        {
            _logger.LogInformation("Attempting to delete device: {device}", device);

            _deviceManager.Delete(device.Id);

            _logger.LogInformation("Finished deleting device");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete device");
        }
    }
}