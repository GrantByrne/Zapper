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

    protected override void OnInitialized()
    {
        _devices.Clear();
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

    private void SaveChanges()
    {
        try
        {
            _logger.LogInformation("Attempting to save changes");

            var webOsName = ConfigureWebOsDevice.Name;
            var ipAddress = ConfigureWebOsDevice.IpAddress;
            var macAddress = ConfigureWebOsDevice.MacAddress;
            _deviceManager.CreateWebOsDevice(webOsName, ipAddress, macAddress);

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
            OnInitialized();

            _logger.LogInformation("Finished deleting device");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete device");
        }
    }
}