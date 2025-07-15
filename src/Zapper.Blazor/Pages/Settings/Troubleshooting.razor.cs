using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using Zapper.Core.Models;

namespace Zapper.Blazor.Pages.Settings;

public partial class Troubleshooting(HttpClient httpClient, ISnackbar snackbar) : ComponentBase
{
    private bool _isTestingTransmitter;
    private bool _isTestingReceiver;
    private bool _isTestingGpioPin;
    private string _troubleshootingMessage = "";
    private SystemInfoResult? _systemInfo;
    private int _testGpioPin = 18;
    private bool _testGpioPinAsOutput = true;

    protected override async Task OnInitializedAsync()
    {
        await LoadSystemInfo();
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