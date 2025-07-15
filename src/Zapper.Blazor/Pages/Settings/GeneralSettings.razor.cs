using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using Zapper.Core.Models;

namespace Zapper.Blazor.Pages.Settings;

public partial class GeneralSettings(HttpClient httpClient, ISnackbar snackbar) : ComponentBase
{
    private bool _enableNotifications = true;
    private bool _enableHapticFeedback = true;
    private bool _enableAutoDiscovery = true;
    private string _defaultActivity = "watch-tv";
    
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
                    _enableNotifications = _currentSettings.General.EnableNotifications;
                    _enableHapticFeedback = _currentSettings.General.EnableHapticFeedback;
                    _enableAutoDiscovery = _currentSettings.General.EnableAutoDiscovery;
                    _defaultActivity = _currentSettings.General.DefaultActivity;
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
            
            _currentSettings.General.EnableNotifications = _enableNotifications;
            _currentSettings.General.EnableHapticFeedback = _enableHapticFeedback;
            _currentSettings.General.EnableAutoDiscovery = _enableAutoDiscovery;
            _currentSettings.General.DefaultActivity = _defaultActivity;

            var response = await httpClient.PutAsJsonAsync("/api/settings", new { Settings = _currentSettings });

            if (response.IsSuccessStatusCode)
            {
                snackbar.Add("General settings saved successfully", Severity.Success);
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