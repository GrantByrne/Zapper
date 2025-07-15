using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using Zapper.Core.Models;

namespace Zapper.Blazor.Pages.Settings;

public partial class HardwareSettings(HttpClient httpClient, ISnackbar snackbar) : ComponentBase
{
    private bool _enableGpio = true;
    private int _irTransmitterPin = 18;
    private int _irReceiverPin = 19;
    private int _carrierFrequency = 38000;
    private double _dutyCycle = 0.33;
    
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
                    _enableGpio = _currentSettings.Hardware.EnableGpio;
                    _irTransmitterPin = _currentSettings.Hardware.Infrared.TransmitterGpioPin;
                    _irReceiverPin = _currentSettings.Hardware.Infrared.ReceiverGpioPin;
                    _carrierFrequency = _currentSettings.Hardware.Infrared.CarrierFrequency;
                    _dutyCycle = _currentSettings.Hardware.Infrared.DutyCycle;
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
            
            _currentSettings.Hardware.EnableGpio = _enableGpio;
            _currentSettings.Hardware.Infrared.TransmitterGpioPin = _irTransmitterPin;
            _currentSettings.Hardware.Infrared.ReceiverGpioPin = _irReceiverPin;
            _currentSettings.Hardware.Infrared.CarrierFrequency = _carrierFrequency;
            _currentSettings.Hardware.Infrared.DutyCycle = _dutyCycle;

            var response = await httpClient.PutAsJsonAsync("/api/settings", new { Settings = _currentSettings });

            if (response.IsSuccessStatusCode)
            {
                snackbar.Add("Hardware settings saved successfully", Severity.Success);
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