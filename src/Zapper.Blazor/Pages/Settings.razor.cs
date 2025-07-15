using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using Zapper.Core.Models;

namespace Zapper.Blazor.Pages;

public partial class Settings(HttpClient httpClient, ISnackbar snackbar) : ComponentBase
{
    private bool _enableNotifications = true;
    private bool _enableHapticFeedback = true;
    private bool _enableAutoDiscovery = true;
    private string _defaultActivity = "watch-tv";
    private int _deviceTimeout = 30;
    private int _retryAttempts = 3;
    private int _irPowerLevel = 5;
    private int _discoveryPort = 1900;
    private int _apiTimeout = 5000;
    private bool _enableSsdp = true;
    private bool _enableDebugLogging;
    private bool _enableTelemetry = true;


    // Hardware Settings
    private bool _enableGpio = true;
    private int _irTransmitterPin = 18;
    private int _irReceiverPin = 19;
    private int _carrierFrequency = 38000;
    private double _dutyCycle = 0.33;

    // Troubleshooting
    private bool _isTestingTransmitter;
    private bool _isTestingReceiver;
    private bool _isTestingGpioPin;
    private string _troubleshootingMessage = "";
    private SystemInfoResult? _systemInfo;
    private int _testGpioPin = 18;
    private bool _testGpioPinAsOutput = true;

    private ZapperSettings? _currentSettings;
    private bool _isLoading = true;
    private bool _isSaving;

    protected override async Task OnInitializedAsync()
    {
        await LoadSettings();
        await LoadSystemInfo();
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
                    MapSettingsToFields(_currentSettings);
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

    private void MapSettingsToFields(ZapperSettings settings)
    {
        _enableNotifications = settings.General.EnableNotifications;
        _enableHapticFeedback = settings.General.EnableHapticFeedback;
        _enableAutoDiscovery = settings.General.EnableAutoDiscovery;
        _defaultActivity = settings.General.DefaultActivity;

        _deviceTimeout = settings.Device.DeviceTimeout;
        _retryAttempts = settings.Device.RetryAttempts;
        _irPowerLevel = settings.Device.IrPowerLevel;

        _discoveryPort = settings.Network.DiscoveryPort;
        _apiTimeout = settings.Network.ApiTimeout;
        _enableSsdp = settings.Network.EnableSsdp;

        _enableGpio = settings.Hardware.EnableGpio;
        _irTransmitterPin = settings.Hardware.Infrared.TransmitterGpioPin;
        _irReceiverPin = settings.Hardware.Infrared.ReceiverGpioPin;
        _carrierFrequency = settings.Hardware.Infrared.CarrierFrequency;
        _dutyCycle = settings.Hardware.Infrared.DutyCycle;

        _enableDebugLogging = settings.Advanced.EnableDebugLogging;
        _enableTelemetry = settings.Advanced.EnableTelemetry;
    }

    private ZapperSettings MapFieldsToSettings()
    {
        return new ZapperSettings
        {
            General = new GeneralSettings
            {
                EnableNotifications = _enableNotifications,
                EnableHapticFeedback = _enableHapticFeedback,
                EnableAutoDiscovery = _enableAutoDiscovery,
                DefaultActivity = _defaultActivity
            },
            Device = new DeviceSettings
            {
                DeviceTimeout = _deviceTimeout,
                RetryAttempts = _retryAttempts,
                IrPowerLevel = _irPowerLevel
            },
            Network = new NetworkSettings
            {
                DiscoveryPort = _discoveryPort,
                ApiTimeout = _apiTimeout,
                EnableSsdp = _enableSsdp
            },
            Hardware = new HardwareSettings
            {
                EnableGpio = _enableGpio,
                Infrared = new IrHardwareSettings
                {
                    TransmitterGpioPin = _irTransmitterPin,
                    ReceiverGpioPin = _irReceiverPin,
                    CarrierFrequency = _carrierFrequency,
                    DutyCycle = _dutyCycle
                }
            },
            Advanced = new AdvancedSettings
            {
                EnableDebugLogging = _enableDebugLogging,
                EnableTelemetry = _enableTelemetry
            }
        };
    }

    private async Task SaveSettings()
    {
        try
        {
            _isSaving = true;
            var settings = MapFieldsToSettings();

            var response = await httpClient.PutAsJsonAsync("/api/settings", new { Settings = settings });

            if (response.IsSuccessStatusCode)
            {
                snackbar.Add("Settings saved successfully", Severity.Success);
                _currentSettings = settings;
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

    private Color GetSystemStatusColor()
    {
        if (_systemInfo == null) return Color.Default;

        if (_systemInfo.IsRaspberryPi && _systemInfo.HasGpioSupport && _systemInfo.GpioWarnings.Count == 0)
            return Color.Success;

        if (_systemInfo.GpioWarnings.Count > 0)
            return Color.Warning;

        return Color.Error;
    }
}

