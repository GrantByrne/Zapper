using System;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Zapper.Core.Devices;
using Zapper.Web.Shared;

namespace Zapper.Web.Pages;

public partial class ManageDevice
{
    [Parameter]
    public Guid DeviceId { get; set; }

    private Device _device;
    private ConfirmationModal _confirmationModal;

    protected override void OnInitialized()
    {
        var device = _deviceManager.Get(DeviceId);
        _device = device;
    }

    private void DisplayConfirmationDialog()
    {
        _confirmationModal.Open();
    }

    private void RemoveDevice()
    {
        try
        {
            _logger.LogInformation($"Attempting to remove device: {_device.Id}");

            _deviceManager.Delete(_device.Id);
            _navigationManager.NavigateTo("/Devices");

            _logger.LogInformation("Successfully removed device");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while trying to remove device");
        }
    }
}