using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using Zapper.Core.Models;

namespace Zapper.Blazor.Pages.Settings;

public partial class AdvancedSettings(HttpClient httpClient, ISnackbar snackbar) : ComponentBase
{
    private bool _enableDebugLogging;
    private bool _enableTelemetry = true;
    
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
                    _enableDebugLogging = _currentSettings.Advanced.EnableDebugLogging;
                    _enableTelemetry = _currentSettings.Advanced.EnableTelemetry;
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
            
            _currentSettings.Advanced.EnableDebugLogging = _enableDebugLogging;
            _currentSettings.Advanced.EnableTelemetry = _enableTelemetry;

            var response = await httpClient.PutAsJsonAsync("/api/settings", new { Settings = _currentSettings });

            if (response.IsSuccessStatusCode)
            {
                snackbar.Add("Advanced settings saved successfully", Severity.Success);
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