using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using Zapper.Core.Models;

namespace Zapper.Blazor.Pages.Settings;

public partial class NetworkSettings(HttpClient httpClient, ISnackbar snackbar) : ComponentBase
{
    private int _discoveryPort = 1900;
    private int _apiTimeout = 5000;
    private bool _enableSsdp = true;
    
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
                    _discoveryPort = _currentSettings.Network.DiscoveryPort;
                    _apiTimeout = _currentSettings.Network.ApiTimeout;
                    _enableSsdp = _currentSettings.Network.EnableSsdp;
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
            
            _currentSettings.Network.DiscoveryPort = _discoveryPort;
            _currentSettings.Network.ApiTimeout = _apiTimeout;
            _currentSettings.Network.EnableSsdp = _enableSsdp;

            var response = await httpClient.PutAsJsonAsync("/api/settings", new { Settings = _currentSettings });

            if (response.IsSuccessStatusCode)
            {
                snackbar.Add("Network settings saved successfully", Severity.Success);
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