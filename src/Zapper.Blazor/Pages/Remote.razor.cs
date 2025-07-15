using Microsoft.AspNetCore.Components;
using Zapper.Client.Abstractions;
using Zapper.Contracts.Devices;
using Zapper.Contracts;

namespace Zapper.Blazor.Pages;

public partial class Remote : ComponentBase
{
    [Inject] public IZapperApiClient? ApiClient { get; set; }

    private List<DeviceDto> _devices = new();
    private int? _selectedDeviceId;
    private bool _isLoading = true;
    private string? _errorMessage;

    protected override async Task OnInitializedAsync()
    {
        await LoadDevices();
    }

    private async Task LoadDevices()
    {
        try
        {
            _isLoading = true;
            _errorMessage = null;

            if (ApiClient == null)
            {
                _errorMessage = "API client not configured";
                return;
            }

            var devices = await ApiClient.Devices.GetAllDevicesAsync();
            _devices = devices.ToList();

            // Select first device by default
            if (_devices.Any())
            {
                _selectedDeviceId = _devices.First().Id;
            }
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to load devices: {ex.Message}";
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private async Task SendCommand(string command)
    {
        if (ApiClient == null || !_selectedDeviceId.HasValue)
        {
            return;
        }

        try
        {
            var request = new SendCommandRequest { Command = command };
            await ApiClient.Devices.SendCommandAsync(_selectedDeviceId.Value, request);
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to send command: {ex.Message}";
            StateHasChanged();
        }
    }

    private async Task SendPowerCommand() => await SendCommand("power");
    private async Task SendUpCommand() => await SendCommand("up");
    private async Task SendDownCommand() => await SendCommand("down");
    private async Task SendLeftCommand() => await SendCommand("left");
    private async Task SendRightCommand() => await SendCommand("right");
    private async Task SendOkCommand() => await SendCommand("ok");
    private async Task SendVolumeUpCommand() => await SendCommand("volume_up");
    private async Task SendVolumeDownCommand() => await SendCommand("volume_down");
    private async Task SendMuteCommand() => await SendCommand("mute");

    private DeviceDto? GetSelectedDevice()
    {
        if (!_selectedDeviceId.HasValue)
        {
            return null;
        }
        return _devices.FirstOrDefault(d => d.Id == _selectedDeviceId.Value);
    }
}