using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using MudBlazor;
using Refit;
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
        PlayStationScan,
        XboxScan,
        RokuScan,
        IrCodeSelection,
        Configuration
    }

    private WizardStep _currentStep = WizardStep.DeviceType;
    private Contracts.ConnectionType? _selectedConnectionType;
    private string _selectedDeviceTypeName = "";
    private bool _isRokuDevice;
    private CreateDeviceRequest _newDevice = new();
    private string _selectedIrCodeSet = "";
    private IrCodeSet? _selectedIrCodeSetData;

    // Bluetooth scanning variables
    private bool _isScanning;
    private string _scanError = "";
    private List<string> _discoveredBluetoothDevices = new();
    private string _selectedBluetoothDevice = "";

    // WebOS scanning variables
    private bool _isWebOsScanning;
    private string _webOsScanError = "";
    private List<WebOsDevice> _discoveredWebOsDevices = new();
    private WebOsDevice? _selectedWebOsDevice;
    private string _manualWebOsIpAddress = "";

    // PlayStation scanning variables
    private bool _isPlayStationScanning;
    private string _playStationScanError = "";
    private List<PlayStationDevice> _discoveredPlayStationDevices = new();
    private PlayStationDevice? _selectedPlayStationDevice;
    private string _manualPlayStationIpAddress = "";

    // Xbox scanning variables
    private bool _isXboxScanning;
    private string _xboxScanError = "";
    private List<XboxDevice> _discoveredXboxDevices = new();
    private XboxDevice? _selectedXboxDevice;
    private string _manualXboxIpAddress = "";

    // Roku scanning variables
    private bool _isRokuScanning;
    private string _rokuScanError = "";
    private List<RokuDevice> _discoveredRokuDevices = new();
    private RokuDevice? _selectedRokuDevice;
    private string _manualRokuIpAddress = "";

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

    private void SelectPlayStationType()
    {
        _selectedDeviceTypeName = "PlayStation";
        _newDevice.Type = Contracts.DeviceType.PlayStation;
        _selectedConnectionType = Contracts.ConnectionType.NetworkTcp;
        _newDevice.ConnectionType = Contracts.ConnectionType.NetworkTcp;
        _currentStep = WizardStep.PlayStationScan;
        _ = StartPlayStationScan();
    }

    private void SelectXboxType()
    {
        _selectedDeviceTypeName = "Xbox";
        _newDevice.Type = Contracts.DeviceType.Xbox;
        _selectedConnectionType = Contracts.ConnectionType.NetworkTcp;
        _newDevice.ConnectionType = Contracts.ConnectionType.NetworkTcp;
        _currentStep = WizardStep.XboxScan;
        _ = StartXboxScan();
    }

    private void ShowConsoleConnectionOptions()
    {
        // For now, we'll default to Bluetooth for modern consoles
        // In the future, we could show a dialog to let users choose
        _selectedConnectionType = Contracts.ConnectionType.Bluetooth;
        _newDevice.ConnectionType = Contracts.ConnectionType.Bluetooth;
        _currentStep = WizardStep.BluetoothScan;
        _ = StartBluetoothScan();
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
            else if (_isRokuDevice)
            {
                _currentStep = WizardStep.RokuScan;
                await StartRokuScan();
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
        else if (_currentStep == WizardStep.PlayStationScan && (_selectedPlayStationDevice != null || !string.IsNullOrWhiteSpace(_manualPlayStationIpAddress)))
        {
            // Pre-populate device info with selected PlayStation
            if (_selectedPlayStationDevice != null)
            {
                if (string.IsNullOrEmpty(_newDevice.Name))
                {
                    _newDevice.Name = _selectedPlayStationDevice.Name;
                }
                _newDevice.IpAddress = _selectedPlayStationDevice.IpAddress;
                _newDevice.Brand = "Sony";
                _newDevice.Model = _selectedPlayStationDevice.Model;
            }
            else if (!string.IsNullOrWhiteSpace(_manualPlayStationIpAddress))
            {
                _newDevice.IpAddress = _manualPlayStationIpAddress.Trim();
                _newDevice.Brand = "Sony";
                _newDevice.Model = "PlayStation";
                if (string.IsNullOrEmpty(_newDevice.Name))
                {
                    _newDevice.Name = $"PlayStation ({_manualPlayStationIpAddress.Trim()})";
                }
            }
            _currentStep = WizardStep.Configuration;
        }
        else if (_currentStep == WizardStep.XboxScan && (_selectedXboxDevice != null || !string.IsNullOrWhiteSpace(_manualXboxIpAddress)))
        {
            // Pre-populate device info with selected Xbox
            if (_selectedXboxDevice != null)
            {
                if (string.IsNullOrEmpty(_newDevice.Name))
                {
                    _newDevice.Name = _selectedXboxDevice.Name;
                }
                _newDevice.IpAddress = _selectedXboxDevice.IpAddress;
                _newDevice.Brand = "Microsoft";
                _newDevice.Model = _selectedXboxDevice.ConsoleType;
            }
            else if (!string.IsNullOrWhiteSpace(_manualXboxIpAddress))
            {
                _newDevice.IpAddress = _manualXboxIpAddress.Trim();
                _newDevice.Brand = "Microsoft";
                _newDevice.Model = "Xbox";
                if (string.IsNullOrEmpty(_newDevice.Name))
                {
                    _newDevice.Name = $"Xbox ({_manualXboxIpAddress.Trim()})";
                }
            }
            _currentStep = WizardStep.Configuration;
        }
        else if (_currentStep == WizardStep.RokuScan && (_selectedRokuDevice != null || !string.IsNullOrWhiteSpace(_manualRokuIpAddress)))
        {
            // Pre-populate device info with selected Roku
            if (_selectedRokuDevice != null)
            {
                if (string.IsNullOrEmpty(_newDevice.Name))
                {
                    _newDevice.Name = _selectedRokuDevice.Name;
                }
                _newDevice.IpAddress = _selectedRokuDevice.IpAddress;
                _newDevice.Brand = "Roku";
                _newDevice.Model = _selectedRokuDevice.Model ?? "Roku Device";
                _newDevice.Type = Contracts.DeviceType.StreamingDevice;
            }
            else if (!string.IsNullOrWhiteSpace(_manualRokuIpAddress))
            {
                _newDevice.IpAddress = _manualRokuIpAddress.Trim();
                _newDevice.Brand = "Roku";
                _newDevice.Model = "Roku Device";
                _newDevice.Type = Contracts.DeviceType.StreamingDevice;
                if (string.IsNullOrEmpty(_newDevice.Name))
                {
                    _newDevice.Name = $"Roku ({_manualRokuIpAddress.Trim()})";
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
            else if (_selectedConnectionType == Contracts.ConnectionType.NetworkTcp && _newDevice.Type == Contracts.DeviceType.PlayStation)
            {
                _currentStep = WizardStep.PlayStationScan;
            }
            else if (_selectedConnectionType == Contracts.ConnectionType.NetworkTcp && _newDevice.Type == Contracts.DeviceType.Xbox)
            {
                _currentStep = WizardStep.XboxScan;
            }
            else if (_isRokuDevice)
            {
                _currentStep = WizardStep.RokuScan;
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
        else if (_currentStep == WizardStep.PlayStationScan)
        {
            await StopPlayStationScan();
            _currentStep = WizardStep.DeviceType;
        }
        else if (_currentStep == WizardStep.XboxScan)
        {
            await StopXboxScan();
            _currentStep = WizardStep.DeviceType;
        }
        else if (_currentStep == WizardStep.RokuScan)
        {
            await StopRokuScan();
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

            // Subscribe to Xbox scanning events
            _hubConnection.On<object>("XboxDeviceFound", (device) =>
            {
                InvokeAsync(() =>
                {
                    var deviceType = device?.GetType();
                    var name = deviceType?.GetProperty("name")?.GetValue(device)?.ToString() ??
                               deviceType?.GetProperty("Name")?.GetValue(device)?.ToString();
                    var ipAddress = deviceType?.GetProperty("ipAddress")?.GetValue(device)?.ToString() ??
                                    deviceType?.GetProperty("IpAddress")?.GetValue(device)?.ToString();
                    var liveId = deviceType?.GetProperty("liveId")?.GetValue(device)?.ToString() ??
                                 deviceType?.GetProperty("LiveId")?.GetValue(device)?.ToString();
                    var consoleType = deviceType?.GetProperty("consoleType")?.GetValue(device)?.ToString() ??
                                      deviceType?.GetProperty("ConsoleType")?.GetValue(device)?.ToString();
                    var isAuthenticated = deviceType?.GetProperty("isAuthenticated")?.GetValue(device) ??
                                          deviceType?.GetProperty("IsAuthenticated")?.GetValue(device);

                    if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(ipAddress))
                    {
                        var xboxDevice = new XboxDevice
                        {
                            Name = name,
                            IpAddress = ipAddress,
                            LiveId = liveId ?? "",
                            ConsoleType = consoleType ?? "Xbox",
                            IsAuthenticated = isAuthenticated as bool? ?? false
                        };

                        if (!_discoveredXboxDevices.Any(d => d.IpAddress == xboxDevice.IpAddress))
                        {
                            _discoveredXboxDevices.Add(xboxDevice);
                            StateHasChanged();
                        }
                    }
                });
            });

            await _hubConnection.StartAsync();
        }
    }

    private void SelectBluetoothDevice(string deviceName)
    {
        _selectedBluetoothDevice = deviceName;
    }

    private async Task StartPlayStationScan()
    {
        if (apiClient == null)
        {
            _playStationScanError = "API client not available. Cannot scan for PlayStation devices.";
            return;
        }

        try
        {
            _isPlayStationScanning = true;
            _playStationScanError = "";
            _discoveredPlayStationDevices.Clear();
            _selectedPlayStationDevice = null;
            _manualPlayStationIpAddress = "";
            StateHasChanged();

            // Initialize SignalR connection if needed
            await EnsureSignalRConnection();

            // Start the scanning process via API
            try
            {
                var request = new DiscoverPlayStationDevicesRequest { TimeoutSeconds = 10 };
                var response = await apiClient.Devices.DiscoverPlayStationDevicesAsync(request);

                if (response != null && response.Any())
                {
                    foreach (var device in response)
                    {
                        _discoveredPlayStationDevices.Add(new PlayStationDevice
                        {
                            Name = device.Name,
                            IpAddress = device.IpAddress,
                            Model = device.Model ?? "PlayStation"
                        });
                    }
                }

                _isPlayStationScanning = false;
                StateHasChanged();
            }
            catch (Refit.ApiException apiEx)
            {
                _playStationScanError = $"API Error: {apiEx.StatusCode} - {apiEx.Message}. Content: {apiEx.Content}";
                _isPlayStationScanning = false;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                _playStationScanError = $"Failed to scan for PlayStation devices: {ex.Message}";
                if (ex.InnerException != null)
                {
                    _playStationScanError += $" Inner: {ex.InnerException.Message}";
                }
                _isPlayStationScanning = false;
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            _playStationScanError = $"Failed to scan for PlayStation devices: {ex.Message}";
            _isPlayStationScanning = false;
            StateHasChanged();
        }
    }

    private void SelectPlayStationDevice(PlayStationDevice device)
    {
        _selectedPlayStationDevice = device;
        _manualPlayStationIpAddress = ""; // Clear manual IP when device is selected
    }

    private void UseManualPlayStationIp()
    {
        if (!string.IsNullOrWhiteSpace(_manualPlayStationIpAddress))
        {
            _selectedPlayStationDevice = null; // Clear selected device when using manual IP
        }
    }

    private async Task StopPlayStationScan()
    {
        _isPlayStationScanning = false;
        _playStationScanError = "";
        await Task.CompletedTask;
        StateHasChanged();
    }

    private async Task StartXboxScan()
    {
        if (apiClient == null)
        {
            _xboxScanError = "API client not available. Cannot scan for Xbox devices.";
            return;
        }

        try
        {
            _isXboxScanning = true;
            _xboxScanError = "";
            _discoveredXboxDevices.Clear();
            _selectedXboxDevice = null;
            _manualXboxIpAddress = "";
            StateHasChanged();

            // Initialize SignalR connection if needed
            await EnsureSignalRConnection();

            // Get SignalR connection ID
            var connectionId = _hubConnection?.ConnectionId ?? "";

            // Start the scanning process via API
            try
            {
                var request = new DiscoverXboxDevicesRequest { DurationSeconds = 15 };
                var headers = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(connectionId))
                {
                    headers["X-SignalR-ConnectionId"] = connectionId;
                }

                var response = await apiClient.Devices.DiscoverXboxDevicesAsync(request);

                if (response.Success && response.Devices != null)
                {
                    foreach (var device in response.Devices)
                    {
                        _discoveredXboxDevices.Add(new XboxDevice
                        {
                            Name = device.Name,
                            IpAddress = device.IpAddress,
                            LiveId = device.LiveId,
                            ConsoleType = device.ConsoleType,
                            IsAuthenticated = device.IsAuthenticated
                        });
                    }
                }

                _isXboxScanning = false;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                _xboxScanError = $"Failed to scan for Xbox devices: {ex.Message}";
                _isXboxScanning = false;
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            _xboxScanError = $"Failed to scan for Xbox devices: {ex.Message}";
            _isXboxScanning = false;
            StateHasChanged();
        }
    }

    private void SelectXboxDevice(XboxDevice device)
    {
        _selectedXboxDevice = device;
        _manualXboxIpAddress = ""; // Clear manual IP when device is selected
    }

    private void UseManualXboxIp()
    {
        if (!string.IsNullOrWhiteSpace(_manualXboxIpAddress))
        {
            _selectedXboxDevice = null; // Clear selected device when using manual IP
        }
    }

    private async Task StopXboxScan()
    {
        _isXboxScanning = false;
        _xboxScanError = "";
        await Task.CompletedTask;
        StateHasChanged();
    }

    private async Task StartRokuScan()
    {
        if (apiClient == null)
        {
            _rokuScanError = "API client not available. Cannot scan for Roku devices.";
            return;
        }

        try
        {
            _isRokuScanning = true;
            _rokuScanError = "";
            _discoveredRokuDevices.Clear();
            _selectedRokuDevice = null;
            _manualRokuIpAddress = "";
            StateHasChanged();

            // Start the scanning process via API
            try
            {
                var request = new DiscoverRokuDevicesRequest { TimeoutSeconds = 10 };
                var response = await apiClient.Devices.DiscoverRokuDevicesAsync(request);

                if (response != null && response.Any())
                {
                    foreach (var device in response)
                    {
                        _discoveredRokuDevices.Add(new RokuDevice
                        {
                            Name = device.Name,
                            IpAddress = device.IpAddress,
                            Model = device.Model,
                            SerialNumber = device.SerialNumber,
                            Port = device.Port
                        });
                    }
                }

                _isRokuScanning = false;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                _rokuScanError = $"Failed to scan for Roku devices: {ex.Message}";
                _isRokuScanning = false;
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            _rokuScanError = $"Failed to scan for Roku devices: {ex.Message}";
            _isRokuScanning = false;
            StateHasChanged();
        }
    }

    private void SelectRokuDevice(RokuDevice device)
    {
        _selectedRokuDevice = device;
        _manualRokuIpAddress = ""; // Clear manual IP when device is selected
    }

    private void UseManualRokuIp()
    {
        if (!string.IsNullOrWhiteSpace(_manualRokuIpAddress))
        {
            _selectedRokuDevice = null; // Clear selected device when using manual IP
        }
    }

    private async Task StopRokuScan()
    {
        _isRokuScanning = false;
        _rokuScanError = "";
        await Task.CompletedTask;
        StateHasChanged();
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

        // Reset PlayStation scanning state
        _isPlayStationScanning = false;
        _playStationScanError = "";
        _discoveredPlayStationDevices.Clear();
        _selectedPlayStationDevice = null;
        _manualPlayStationIpAddress = "";

        // Reset Xbox scanning state
        _isXboxScanning = false;
        _xboxScanError = "";
        _discoveredXboxDevices.Clear();
        _selectedXboxDevice = null;
        _manualXboxIpAddress = "";

        // Reset Roku scanning state
        _isRokuScanning = false;
        _rokuScanError = "";
        _discoveredRokuDevices.Clear();
        _selectedRokuDevice = null;
        _manualRokuIpAddress = "";
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
        else if (_currentStep == WizardStep.PlayStationScan && _isPlayStationScanning)
        {
            await StopPlayStationScan();
        }
        else if (_currentStep == WizardStep.XboxScan && _isXboxScanning)
        {
            await StopXboxScan();
        }
        else if (_currentStep == WizardStep.RokuScan && _isRokuScanning)
        {
            await StopRokuScan();
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
            Contracts.DeviceType.PlayStation => DeviceType.PlayStation,
            Contracts.DeviceType.Xbox => DeviceType.Xbox,
            Contracts.DeviceType.Receiver => DeviceType.Receiver,
            Contracts.DeviceType.DvdPlayer => DeviceType.DvdPlayer,
            Contracts.DeviceType.BluRayPlayer => DeviceType.BluRayPlayer,
            _ => DeviceType.Television
        };
    }

    private class PlayStationDevice
    {
        public string Name { get; set; } = "";
        public string IpAddress { get; set; } = "";
        public string Model { get; set; } = "";
    }

    private class XboxDevice
    {
        public string Name { get; set; } = "";
        public string IpAddress { get; set; } = "";
        public string LiveId { get; set; } = "";
        public string ConsoleType { get; set; } = "";
        public bool IsAuthenticated { get; set; }
    }

    private class RokuDevice
    {
        public string Name { get; set; } = "";
        public string IpAddress { get; set; } = "";
        public string? Model { get; set; }
        public string? SerialNumber { get; set; }
        public int Port { get; set; } = 8060;
    }
}