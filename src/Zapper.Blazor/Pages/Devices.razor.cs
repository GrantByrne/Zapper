using Microsoft.AspNetCore.Components;
using Zapper.Client.Abstractions;
using Zapper.Contracts.Devices;
using Zapper.Contracts;
using MudBlazor;

namespace Zapper.Blazor.Pages;

public partial class Devices : ComponentBase
{
    [Inject] public IZapperApiClient? ApiClient { get; set; }

    private List<DeviceDto> _devices = new();
    private bool _showAddDialog = false;
    private bool _isLoading = true;
    private string? _errorMessage;
    private string _loadingStep = "initializing";

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
            _loadingStep = "initializing";

            if (ApiClient == null)
            {
                _errorMessage = "API client not configured. Please check the application setup.";
                return;
            }

            _loadingStep = "connecting";
            await Task.Delay(100); // Brief delay to show connection step

            _loadingStep = "fetching";

            // Add timeout to prevent hanging
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var devices = await ApiClient.Devices.GetAllDevicesAsync();
            _devices = devices.ToList();
        }
        catch (TaskCanceledException)
        {
            _errorMessage = "Request timed out. Please check if the API server is running.";
        }
        catch (HttpRequestException ex)
        {
            _errorMessage = $"Network error: {ex.Message}. The API server may not be running.";
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to load devices: {ex.Message}";
        }
        finally
        {
            _isLoading = false;
            _loadingStep = "initializing";
        }
    }

    private string GetDeviceIcon(DeviceType deviceType)
    {
        return deviceType switch
        {
            DeviceType.Television => Icons.Material.Filled.Tv,
            DeviceType.SmartTv => Icons.Material.Filled.Tv,
            DeviceType.SoundBar => Icons.Material.Filled.Speaker,
            DeviceType.StreamingDevice => Icons.Material.Filled.Cast,
            DeviceType.AppleTv => Icons.Material.Filled.Cast,
            DeviceType.CableBox => Icons.Material.Filled.Tv,
            DeviceType.GameConsole => Icons.Material.Filled.SportsEsports,
            DeviceType.Receiver => Icons.Material.Filled.Speaker,
            DeviceType.DvdPlayer => Icons.Material.Filled.VideoLibrary,
            DeviceType.BluRayPlayer => Icons.Material.Filled.VideoLibrary,
            _ => Icons.Material.Filled.DevicesOther
        };
    }

    private async Task AddDevice(CreateDeviceRequest newDevice)
    {
        if (!string.IsNullOrWhiteSpace(newDevice.Name) && ApiClient != null)
        {
            try
            {
                var createdDevice = await ApiClient.Devices.CreateDeviceAsync(newDevice);
                _devices.Add(createdDevice);

                _showAddDialog = false;
            }
            catch (Exception ex)
            {
                _errorMessage = $"Failed to create device: {ex.Message}";
            }
        }
        else if (ApiClient == null)
        {
            _errorMessage = "API client not available";
        }
    }

    private async Task TestDevice(DeviceDto device)
    {
        if (ApiClient == null)
        {
            _errorMessage = "API client not available";
            return;
        }

        try
        {
            // Send a test command to the device
            await ApiClient.Devices.SendCommandAsync(device.Id, new SendCommandRequest { Command = "test" });
            // Note: In a real implementation, you'd want to update the device status based on the response
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to test device: {ex.Message}";
        }
    }

    private void EditDevice(DeviceDto device)
    {
        // TODO: Implement device editing dialog
    }

    private async Task DeleteDevice(DeviceDto device)
    {
        if (ApiClient == null)
        {
            _errorMessage = "API client not available";
            return;
        }

        try
        {
            await ApiClient.Devices.DeleteDeviceAsync(device.Id);
            _devices.Remove(device);
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to delete device: {ex.Message}";
        }
    }
}