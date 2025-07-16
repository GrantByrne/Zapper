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

    private bool _isTestingTransmitter;
    private bool _isTestingReceiver;
    private bool _isTestingGpioPin;
    private string _troubleshootingMessage = "";
    private SystemInfoResult? _systemInfo;
    private int _testGpioPin = 18;
    private bool _testGpioPinAsOutput = true;

    protected override async Task OnInitializedAsync()
    {
        await Task.WhenAll(LoadSettings(), LoadSystemInfo());
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

    private async Task LoadSystemInfo()
    {
        try
        {
            var response = await httpClient.GetAsync("/api/system/info");
            if (response.IsSuccessStatusCode)
            {
                _systemInfo = await response.Content.ReadFromJsonAsync<SystemInfoResult>();
            }
        }
        catch (Exception ex)
        {
            snackbar.Add($"Error loading system info: {ex.Message}", Severity.Warning);
        }
    }

    private async Task TestIrTransmitter()
    {
        try
        {
            _isTestingTransmitter = true;
            _troubleshootingMessage = "Testing IR transmitter...";

            var response = await httpClient.PostAsync("/api/ir-codes/test-transmitter", null);
            var result = await response.Content.ReadFromJsonAsync<IrHardwareTestResult>();

            if (result != null)
            {
                _troubleshootingMessage = result.Message;
                snackbar.Add(result.Message, result.TestPassed ? Severity.Success : Severity.Warning);
            }
        }
        catch (Exception ex)
        {
            _troubleshootingMessage = $"Test failed: {ex.Message}";
            snackbar.Add(_troubleshootingMessage, Severity.Error);
        }
        finally
        {
            _isTestingTransmitter = false;
        }
    }

    private async Task TestIrReceiver()
    {
        try
        {
            _isTestingReceiver = true;
            _troubleshootingMessage = "Testing IR receiver - point a remote at the receiver and press any button...";

            var request = new { TimeoutSeconds = 15 };
            var response = await httpClient.PostAsJsonAsync("/api/ir-codes/test-receiver", request);
            var result = await response.Content.ReadFromJsonAsync<IrHardwareTestResult>();

            if (result != null)
            {
                _troubleshootingMessage = result.Message;
                snackbar.Add(result.Message, result.TestPassed ? Severity.Success : Severity.Warning);
            }
        }
        catch (Exception ex)
        {
            _troubleshootingMessage = $"Test failed: {ex.Message}";
            snackbar.Add(_troubleshootingMessage, Severity.Error);
        }
        finally
        {
            _isTestingReceiver = false;
        }
    }

    private async Task TestGpioPin()
    {
        try
        {
            _isTestingGpioPin = true;
            _troubleshootingMessage = $"Testing GPIO pin {_testGpioPin}...";

            var request = new { Pin = _testGpioPin, IsOutput = _testGpioPinAsOutput };
            var response = await httpClient.PostAsJsonAsync("/api/system/test-gpio-pin", request);
            var result = await response.Content.ReadFromJsonAsync<GpioTestResult>();

            if (result != null)
            {
                _troubleshootingMessage = result.Message;
                snackbar.Add(result.Message, result.CanAccess ? Severity.Success : Severity.Warning);
            }
        }
        catch (Exception ex)
        {
            _troubleshootingMessage = $"Test failed: {ex.Message}";
            snackbar.Add(_troubleshootingMessage, Severity.Error);
        }
        finally
        {
            _isTestingGpioPin = false;
        }
    }
}