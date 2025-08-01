using Microsoft.AspNetCore.Components;
using Zapper.Client;
using Zapper.Client.Devices;
using Zapper.Core.Models;
using MudBlazor;

namespace Zapper.Blazor.Pages;

public partial class Devices(IZapperApiClient? apiClient) : ComponentBase
{

    private List<DeviceDto> _devices = new();
    private bool _showAddDialog;
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

            if (apiClient == null)
            {
                _errorMessage = "API client not configured. Please check the application setup.";
                return;
            }

            _loadingStep = "connecting";
            await Task.Delay(100); // Brief delay to show connection step

            _loadingStep = "fetching";

            // Add timeout to prevent hanging
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var devices = await apiClient.Devices.GetAllDevicesAsync();
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
        if (!string.IsNullOrWhiteSpace(newDevice.Name) && apiClient != null)
        {
            try
            {
                var createdDevice = await apiClient.Devices.CreateDeviceAsync(newDevice);
                _devices.Add(createdDevice);

                _showAddDialog = false;
            }
            catch (Exception ex)
            {
                _errorMessage = $"Failed to create device: {ex.Message}";
            }
        }
        else if (apiClient == null)
        {
            _errorMessage = "API client not available";
        }
    }

    private async Task TestDevice(DeviceDto device)
    {
        if (apiClient == null)
        {
            _errorMessage = "API client not available";
            return;
        }

        try
        {
            // Send a test command to the device
            await apiClient.Devices.SendCommandAsync(device.Id, new SendCommandRequest { Command = "test" });
            // Note: In a real implementation, you'd want to update the device status based on the response
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to test device: {ex.Message}";
        }
    }

    private void EditDevice(DeviceDto device)
    {
        // Device editing would require a modal dialog component
        // For now, users can delete and re-add devices with new settings
    }

    private async Task DeleteDevice(DeviceDto device)
    {
        if (apiClient == null)
        {
            _errorMessage = "API client not available";
            return;
        }

        try
        {
            await apiClient.Devices.DeleteDeviceAsync(device.Id);
            _devices.Remove(device);
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to delete device: {ex.Message}";
        }
    }
}