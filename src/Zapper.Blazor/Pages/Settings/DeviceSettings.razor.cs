using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using Zapper.Core.Models;

namespace Zapper.Blazor.Pages.Settings;

public partial class DeviceSettings(HttpClient httpClient, ISnackbar snackbar) : ComponentBase
{
    private int _deviceTimeout = 30;
    private int _retryAttempts = 3;
    private int _irPowerLevel = 5;

    private ZapperSettings? _currentSettings;
    private bool _isLoading = true;
    private bool _isSaving;

    protected override async Task OnInitializedAsync()
    {
        await LoadSettings();
    }

    private async Task LoadSettings()
    {
        try
        {
            _isLoading = true;
            var response = await httpClient.GetAsync("/api/settings");
            if (response.IsSuccessStatusCode)
            {
                _currentSettings = await response.Content.ReadFromJsonAsync<ZapperSettings>();
                if (_currentSettings != null)
                {
                    _deviceTimeout = _currentSettings.Device.DeviceTimeout;
                    _retryAttempts = _currentSettings.Device.RetryAttempts;
                    _irPowerLevel = _currentSettings.Device.IrPowerLevel;
                }
            }
        }
        catch (Exception ex)
        {
            snackbar.Add($"Error loading settings: {ex.Message}", Severity.Error);
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task SaveSettings()
    {
        try
        {
            _isSaving = true;

            if (_currentSettings == null)
            {
                _currentSettings = new ZapperSettings();
            }

            _currentSettings.Device.DeviceTimeout = _deviceTimeout;
            _currentSettings.Device.RetryAttempts = _retryAttempts;
            _currentSettings.Device.IrPowerLevel = _irPowerLevel;

            var response = await httpClient.PutAsJsonAsync("/api/settings", new { Settings = _currentSettings });

            if (response.IsSuccessStatusCode)
            {
                snackbar.Add("Device settings saved successfully", Severity.Success);
            }
            else
            {
                snackbar.Add("Failed to save settings", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            snackbar.Add($"Error saving settings: {ex.Message}", Severity.Error);
        }
        finally
        {
            _isSaving = false;
        }
    }
}