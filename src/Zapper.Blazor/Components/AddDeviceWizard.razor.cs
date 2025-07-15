using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using MudBlazor;
using Zapper.Client;
using Zapper.Contracts.Devices;
using Zapper.Core.Models;

namespace Zapper.Blazor.Components;

public partial class AddDeviceWizard(IZapperApiClient? apiClient, IJSRuntime jsRuntime) : ComponentBase, IAsyncDisposable
{
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }
    [Parameter] public EventCallback<CreateDeviceRequest> OnDeviceAdded { get; set; }

    private enum WizardStep
    {
        DeviceType,
        BluetoothScan,
        WebOsScan,
        IrCodeSelection,
        Configuration
    }

    private WizardStep _currentStep = WizardStep.DeviceType;
    private Contracts.ConnectionType? _selectedConnectionType;
    private string _selectedDeviceTypeName = "";
    private bool _isRokuDevice = false;
    private CreateDeviceRequest _newDevice = new();
    private string _selectedIrCodeSet = "";
    private IrCodeSet? _selectedIrCodeSetData;

    // Bluetooth scanning variables
    private bool _isScanning = false;
    private string _scanError = "";
    private List<string> _discoveredBluetoothDevices = new();
    private string _selectedBluetoothDevice = "";

    // WebOS scanning variables
    private bool _isWebOsScanning = false;
    private string _webOsScanError = "";
    private List<WebOsDevice> _discoveredWebOsDevices = new();
    private WebOsDevice? _selectedWebOsDevice = null;
    private string _manualWebOsIpAddress = "";

    // SignalR connection for real-time updates
    private HubConnection? _hubConnection;

    private DialogOptions _dialogOptions = new() { MaxWidth = MaxWidth.Medium, FullWidth = true };

    private void SelectDeviceType(Contracts.ConnectionType connectionType, string typeName, bool isRoku = false)
    {
        _selectedConnectionType = connectionType;
        _selectedDeviceTypeName = typeName;
        _isRokuDevice = isRoku;
        _newDevice.ConnectionType = connectionType;
    }

    private async Task SelectDeviceTypeAndProceed(Contracts.ConnectionType connectionType, string typeName, bool isRoku = false)
    {
        SelectDeviceType(connectionType, typeName, isRoku);
        await NextStep();
    }

    private string GetCardClass(Contracts.ConnectionType connectionType, bool isRoku = false)
    {
        bool isSelected = _selectedConnectionType == connectionType && (_isRokuDevice == isRoku || !isRoku);
        return $"device-type-card {(isSelected ? "selected" : "")}";
    }

    private async Task NextStep()
    {
        if (_currentStep == WizardStep.DeviceType && _selectedConnectionType.HasValue)
        {
            if (_selectedConnectionType == Contracts.ConnectionType.Bluetooth)
            {
                _currentStep = WizardStep.BluetoothScan;
                await StartBluetoothScan();
            }
            else if (_selectedConnectionType == Contracts.ConnectionType.WebOs)
            {
                _currentStep = WizardStep.WebOsScan;
                await StartWebOsScan();
            }
            else if (_selectedConnectionType == Contracts.ConnectionType.InfraredIr)
            {
                _currentStep = WizardStep.IrCodeSelection;
            }
            else
            {
                _currentStep = WizardStep.Configuration;
            }
        }
        else if (_currentStep == WizardStep.BluetoothScan && !string.IsNullOrEmpty(_selectedBluetoothDevice))
        {
            // Pre-populate device name with selected Bluetooth device
            if (string.IsNullOrEmpty(_newDevice.Name))
            {
                _newDevice.Name = _selectedBluetoothDevice;
            }
            _currentStep = WizardStep.Configuration;
        }
        else if (_currentStep == WizardStep.WebOsScan && (_selectedWebOsDevice != null || !string.IsNullOrWhiteSpace(_manualWebOsIpAddress)))
        {
            // Pre-populate device info with selected WebOS TV
            if (_selectedWebOsDevice != null)
            {
                if (string.IsNullOrEmpty(_newDevice.Name))
                {
                    _newDevice.Name = _selectedWebOsDevice.Name;
                }
                _newDevice.IpAddress = _selectedWebOsDevice.IpAddress;
                _newDevice.Brand = "LG";
                _newDevice.Model = _selectedWebOsDevice.ModelName ?? "";
            }
            else if (!string.IsNullOrWhiteSpace(_manualWebOsIpAddress))
            {
                _newDevice.IpAddress = _manualWebOsIpAddress.Trim();
                _newDevice.Brand = "LG";
                if (string.IsNullOrEmpty(_newDevice.Name))
                {
                    _newDevice.Name = $"WebOS TV ({_manualWebOsIpAddress.Trim()})";
                }
            }
            _currentStep = WizardStep.Configuration;
        }
        else if (_currentStep == WizardStep.IrCodeSelection && _selectedIrCodeSetData != null)
        {
            // Store the selected IR code set ID
            _selectedIrCodeSet = _selectedIrCodeSetData.Id.ToString();
            if (string.IsNullOrEmpty(_newDevice.Brand))
            {
                _newDevice.Brand = _selectedIrCodeSetData.Brand;
            }
            if (string.IsNullOrEmpty(_newDevice.Model))
            {
                _newDevice.Model = _selectedIrCodeSetData.Model;
            }
            _currentStep = WizardStep.Configuration;
        }
    }

    private async Task PreviousStep()
    {
        if (_currentStep == WizardStep.Configuration)
        {
            if (_selectedConnectionType == Contracts.ConnectionType.Bluetooth)
            {
                _currentStep = WizardStep.BluetoothScan;
            }
            else if (_selectedConnectionType == Contracts.ConnectionType.WebOs)
            {
                _currentStep = WizardStep.WebOsScan;
            }
            else if (_selectedConnectionType == Contracts.ConnectionType.InfraredIr)
            {
                _currentStep = WizardStep.IrCodeSelection;
            }
            else
            {
                _currentStep = WizardStep.DeviceType;
            }
        }
        else if (_currentStep == WizardStep.BluetoothScan)
        {
            await StopBluetoothScan();
            _currentStep = WizardStep.DeviceType;
        }
        else if (_currentStep == WizardStep.WebOsScan)
        {
            await StopWebOsScan();
            _currentStep = WizardStep.DeviceType;
        }
        else if (_currentStep == WizardStep.IrCodeSelection)
        {
            _currentStep = WizardStep.DeviceType;
        }
    }

    private async Task StartBluetoothScan()
    {
        if (apiClient == null)
        {
            _scanError = "API client not available. Cannot scan for Bluetooth devices.";
            return;
        }

        try
        {
            _isScanning = true;
            _scanError = "";
            _discoveredBluetoothDevices.Clear();
            _selectedBluetoothDevice = "";
            StateHasChanged();

            // Initialize SignalR connection if needed
            await EnsureSignalRConnection();

            // Start the scanning process via API
            try
            {
                var scanRequest = new BluetoothScanRequest { DurationSeconds = 30 };
                var response = await apiClient.Devices.StartBluetoothScanAsync(scanRequest);

                if (!response.Success)
                {
                    _scanError = response.Message ?? "Failed to start Bluetooth scan";
                    _isScanning = false;
                    StateHasChanged();
                }
                // Note: _isScanning will be set to false by SignalR BluetoothScanCompleted event
            }
            catch (Exception ex)
            {
                // Fallback to the old discovery method if new endpoint fails
                _scanError = $"New scanning failed ({ex.Message}), using fallback method...";
                StateHasChanged();
                await Task.Delay(500);

                var devices = await apiClient.Devices.DiscoverBluetoothDevicesAsync();
                _discoveredBluetoothDevices = devices.ToList();
                _isScanning = false;
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            _scanError = $"Failed to scan for Bluetooth devices: {ex.Message}";
            _isScanning = false;
            StateHasChanged();
        }
    }

    private async Task EnsureSignalRConnection()
    {
        if (_hubConnection == null)
        {
            var baseUri = await jsRuntime.InvokeAsync<string>("eval", "window.location.origin");
            _hubConnection = new HubConnectionBuilder()
                .WithUrl($"{baseUri}/hubs/zapper")
                .WithAutomaticReconnect()
                .Build();

            // Subscribe to Bluetooth scanning events
            _hubConnection.On("BluetoothScanStarted", () =>
            {
                InvokeAsync(() =>
                {
                    _isScanning = true;
                    StateHasChanged();
                });
            });

            _hubConnection.On<object>("BluetoothDeviceFound", (device) =>
            {
                InvokeAsync(() =>
                {
                    var deviceName = device?.GetType().GetProperty("Name")?.GetValue(device)?.ToString();
                    if (!string.IsNullOrEmpty(deviceName) && !_discoveredBluetoothDevices.Contains(deviceName))
                    {
                        _discoveredBluetoothDevices.Add(deviceName);
                        StateHasChanged();
                    }
                });
            });

            _hubConnection.On("BluetoothScanCompleted", () =>
            {
                InvokeAsync(() =>
                {
                    _isScanning = false;
                    StateHasChanged();
                });
            });

            _hubConnection.On<string>("BluetoothScanError", (error) =>
            {
                InvokeAsync(() =>
                {
                    _scanError = error;
                    _isScanning = false;
                    StateHasChanged();
                });
            });

            // Subscribe to WebOS scanning events
            _hubConnection.On("WebOSScanStarted", () =>
            {
                InvokeAsync(() =>
                {
                    _isWebOsScanning = true;
                    StateHasChanged();
                });
            });

            _hubConnection.On<object>("WebOSDeviceFound", (device) =>
            {
                InvokeAsync(() =>
                {
                    var deviceType = device?.GetType();
                    var name = deviceType?.GetProperty("Name")?.GetValue(device)?.ToString();
                    var ipAddress = deviceType?.GetProperty("IpAddress")?.GetValue(device)?.ToString();
                    var modelName = deviceType?.GetProperty("ModelName")?.GetValue(device)?.ToString();
                    var modelNumber = deviceType?.GetProperty("ModelNumber")?.GetValue(device)?.ToString();
                    var port = deviceType?.GetProperty("Port")?.GetValue(device)?.ToString();

                    if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(ipAddress))
                    {
                        var webOsDevice = new WebOsDevice
                        {
                            Name = name,
                            IpAddress = ipAddress,
                            ModelName = modelName,
                            ModelNumber = modelNumber,
                            Port = !string.IsNullOrEmpty(port) ? int.Parse(port) : 3000
                        };

                        if (!_discoveredWebOsDevices.Any(d => d.IpAddress == webOsDevice.IpAddress))
                        {
                            _discoveredWebOsDevices.Add(webOsDevice);
                            StateHasChanged();
                        }
                    }
                });
            });

            _hubConnection.On("WebOSScanCompleted", () =>
            {
                InvokeAsync(() =>
                {
                    _isWebOsScanning = false;
                    StateHasChanged();
                });
            });

            _hubConnection.On<string>("WebOSScanError", (error) =>
            {
                InvokeAsync(() =>
                {
                    _webOsScanError = error;
                    _isWebOsScanning = false;
                    StateHasChanged();
                });
            });

            await _hubConnection.StartAsync();
        }
    }

    private void SelectBluetoothDevice(string deviceName)
    {
        _selectedBluetoothDevice = deviceName;
    }

    private async Task StartWebOsScan()
    {
        if (apiClient == null)
        {
            _webOsScanError = "API client not available. Cannot scan for WebOS TVs.";
            return;
        }

        try
        {
            _isWebOsScanning = true;
            _webOsScanError = "";
            _discoveredWebOsDevices.Clear();
            _selectedWebOsDevice = null;
            _manualWebOsIpAddress = "";
            StateHasChanged();

            // Initialize SignalR connection if needed
            await EnsureSignalRConnection();

            // Start the scanning process via API
            try
            {
                var scanRequest = new WebOsScanRequest { DurationSeconds = 15 };
                var response = await apiClient.Devices.StartWebOsScanAsync(scanRequest);

                if (!response.Success)
                {
                    _webOsScanError = response.Message ?? "Failed to start WebOS TV scan";
                    _isWebOsScanning = false;
                    StateHasChanged();
                }
                // Note: _isWebOSScanning will be set to false by SignalR WebOSScanCompleted event
            }
            catch (Exception ex)
            {
                _webOsScanError = $"Failed to start WebOS TV scan: {ex.Message}";
                _isWebOsScanning = false;
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            _webOsScanError = $"Failed to scan for WebOS TVs: {ex.Message}";
            _isWebOsScanning = false;
            StateHasChanged();
        }
    }

    private void SelectWebOsDevice(WebOsDevice device)
    {
        _selectedWebOsDevice = device;
        _manualWebOsIpAddress = ""; // Clear manual IP when device is selected
    }

    private void UseManualWebOsip()
    {
        if (!string.IsNullOrWhiteSpace(_manualWebOsIpAddress))
        {
            _selectedWebOsDevice = null; // Clear selected device when using manual IP
        }
    }

    private void HandleIrCodeSetSelected(IrCodeSet codeSet)
    {
        _selectedIrCodeSetData = codeSet;
    }

    private async Task FinishWizard()
    {
        if (!string.IsNullOrWhiteSpace(_newDevice.Name))
        {
            // Store the IR code set ID in device metadata or custom field
            if (_selectedConnectionType == Contracts.ConnectionType.InfraredIr && _selectedIrCodeSetData != null)
            {
                // Store IR code set ID - this will need to be handled in the API
                _newDevice.IrCodeSetId = _selectedIrCodeSetData.Id;
            }

            await OnDeviceAdded.InvokeAsync(_newDevice);
            ResetWizard();
        }
    }

    private void ResetWizardState()
    {
        _currentStep = WizardStep.DeviceType;
        _selectedConnectionType = null;
        _selectedDeviceTypeName = "";
        _isRokuDevice = false;
        _newDevice = new CreateDeviceRequest();
        _selectedIrCodeSet = "";
        _selectedIrCodeSetData = null;

        // Reset Bluetooth scanning state
        _isScanning = false;
        _scanError = "";
        _discoveredBluetoothDevices.Clear();
        _selectedBluetoothDevice = "";

        // Reset WebOS scanning state
        _isWebOsScanning = false;
        _webOsScanError = "";
        _discoveredWebOsDevices.Clear();
        _selectedWebOsDevice = null;
        _manualWebOsIpAddress = "";
    }

    private void ResetWizard()
    {
        ResetWizardState();
        IsVisible = false;
        _ = IsVisibleChanged.InvokeAsync(false);
    }

    private async Task CancelWizard()
    {
        // Stop any ongoing scans before canceling
        if (_currentStep == WizardStep.BluetoothScan && _isScanning)
        {
            await StopBluetoothScan();
        }
        else if (_currentStep == WizardStep.WebOsScan && _isWebOsScanning)
        {
            await StopWebOsScan();
        }

        ResetWizard();
    }

    private async Task StopBluetoothScan()
    {
        if (_isScanning && apiClient != null)
        {
            try
            {
                var response = await apiClient.Devices.StopBluetoothScanAsync();

                if (response.Success)
                {
                    _isScanning = false;
                    _scanError = "";
                }
                else
                {
                    _scanError = response.Message ?? "Failed to stop Bluetooth scan";
                    _isScanning = false;
                }
                StateHasChanged();
            }
            catch (Exception ex)
            {
                _scanError = $"Failed to stop Bluetooth scan: {ex.Message}";
                _isScanning = false;
                StateHasChanged();
            }
        }
        else if (_isScanning)
        {
            // Fallback if API client is not available
            _isScanning = false;
            _scanError = "";
            StateHasChanged();
        }
    }

    private async Task StopWebOsScan()
    {
        if (_isWebOsScanning && apiClient != null)
        {
            try
            {
                var response = await apiClient.Devices.StopWebOsScanAsync();

                if (response.Success)
                {
                    _isWebOsScanning = false;
                    _webOsScanError = "";
                }
                else
                {
                    _webOsScanError = response.Message ?? "Failed to stop WebOS scan";
                    _isWebOsScanning = false;
                }
                StateHasChanged();
            }
            catch (Exception ex)
            {
                _webOsScanError = $"Failed to stop WebOS scan: {ex.Message}";
                _isWebOsScanning = false;
                StateHasChanged();
            }
        }
        else if (_isWebOsScanning)
        {
            // Fallback if API client is not available
            _isWebOsScanning = false;
            _webOsScanError = "";
            StateHasChanged();
        }
    }

    protected override void OnParametersSet()
    {
        if (!IsVisible && _currentStep != WizardStep.DeviceType)
        {
            ResetWizardState();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
    }

    private static DeviceType MapToCoreDeviceType(Contracts.DeviceType deviceType)
    {
        return deviceType switch
        {
            Contracts.DeviceType.Television => DeviceType.Television,
            Contracts.DeviceType.SmartTv => DeviceType.SmartTv,
            Contracts.DeviceType.SoundBar => DeviceType.SoundBar,
            Contracts.DeviceType.StreamingDevice => DeviceType.StreamingDevice,
            Contracts.DeviceType.AppleTv => DeviceType.AppleTv,
            Contracts.DeviceType.CableBox => DeviceType.CableBox,
            Contracts.DeviceType.GameConsole => DeviceType.GameConsole,
            Contracts.DeviceType.Receiver => DeviceType.Receiver,
            Contracts.DeviceType.DvdPlayer => DeviceType.DvdPlayer,
            Contracts.DeviceType.BluRayPlayer => DeviceType.BluRayPlayer,
            _ => DeviceType.Television
        };
    }
}