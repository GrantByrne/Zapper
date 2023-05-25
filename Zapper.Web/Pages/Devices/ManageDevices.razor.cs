using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Zapper.Web.Data;
using Zapper.Web.Shared;

namespace Zapper.Web.Pages.Devices;

public partial class ManageDevices
{
    private readonly List<DeviceModel> _devices = new();
    private ConfirmationModal _confirmationModal;
    private DeviceModel _forDelete;

    private CreateWebOsDeviceModal CreateWebOsDeviceModel { get; set; }

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
        
        StateHasChanged();
    }

    private void AddDevice()
    {
        try
        {
            _logger.LogInformation("Bring up the modal to add a new device");
            CreateWebOsDeviceModel.Open();
            _logger.LogInformation("Brought up the modal to add a new device");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to bring up the modal for a new device");
        }
    }

    private void ConfirmDelete(DeviceModel device)
    {
        _forDelete = device;
        _confirmationModal.Open();
    }

    private void Delete()
    {
        try
        {
            _logger.LogInformation("Attempting to delete device: {device}", _forDelete);

            _deviceManager.Delete(_forDelete.Id);
            OnInitialized();

            _logger.LogInformation("Finished deleting device");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete device");
        }
    }
}