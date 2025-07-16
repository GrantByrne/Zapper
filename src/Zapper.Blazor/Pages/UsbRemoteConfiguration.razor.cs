using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor;
using System.Net.Http.Json;
using System.Timers;
using Zapper.Client;
using Zapper.Core.Models;
using Zapper.Client.Devices;

namespace Zapper.Blazor.Pages;

public partial class UsbRemoteConfiguration(IZapperApiClient? apiClient, HttpClient httpClient, NavigationManager navigation) : ComponentBase, IAsyncDisposable
{
    [Parameter] public int RemoteId { get; set; }

    private UsbRemote? _remote;
    private List<UsbRemoteButton> _buttons = new();
    private List<DeviceDto> _devices = new();
    private List<DeviceCommand> _deviceCommands = new();
    private List<ButtonEventData> _buttonEvents = new();
    private bool _isLoading = true;
    private string? _errorMessage;
    private HubConnection? _hubConnection;
    private System.Timers.Timer? _learningTimer;

    private bool _showLearningDialog;
    private bool _isLearning;
    private int _learningTimeLeft = 10;
    private UsbRemoteButton? _learnedButton;

    private bool _showAddMappingDialog;
    private CreateButtonMappingModel _newMapping = new();
    private MudForm? _mappingForm;

    private readonly DialogOptions _dialogOptions = new()
    {
        CloseButton = true,
        MaxWidth = MaxWidth.Small,
        FullWidth = true
    };

    private readonly List<BreadcrumbItem> _breadcrumbs = new()
    {
        new BreadcrumbItem("USB Remotes", href: "/usb-remotes"),
        new BreadcrumbItem("Configure", href: null, disabled: true)
    };

    protected override async Task OnInitializedAsync()
    {
        await LoadData();
        await SetupSignalRConnection();
    }

    private async Task LoadData()
    {
        try
        {
            _isLoading = true;
            _errorMessage = null;

            // Load remote details
            var remote = await httpClient.GetFromJsonAsync<UsbRemote>($"/api/usb-remotes/{RemoteId}");
            _remote = remote;

            if (_remote == null)
            {
                _errorMessage = "USB remote not found";
                return;
            }

            // Load buttons
            var buttons = await httpClient.GetFromJsonAsync<IEnumerable<UsbRemoteButton>>($"/api/usb-remotes/{RemoteId}/buttons");
            _buttons = buttons?.ToList() ?? new List<UsbRemoteButton>();

            // Load devices for mapping
            if (apiClient != null)
            {
                var devices = await apiClient.Devices.GetAllDevicesAsync();
                _devices = devices.ToList();
            }
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to load configuration: {ex.Message}";
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private async Task SetupSignalRConnection()
    {
        try
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(navigation.ToAbsoluteUri("/hubs/zapper"))
                .Build();

            _hubConnection.On<UsbRemoteButtonEventData>("UsbRemoteButtonPressed", (data) =>
            {
                if (data.DeviceId == _remote?.DeviceId)
                {
                    _buttonEvents.Add(new ButtonEventData
                    {
                        ButtonName = data.ButtonName,
                        EventType = data.EventType,
                        Timestamp = DateTime.UtcNow
                    });

                    // Keep only last 20 events
                    if (_buttonEvents.Count > 20)
                    {
                        _buttonEvents.RemoveAt(0);
                    }

                    InvokeAsync(StateHasChanged);
                }
            });

            await _hubConnection.StartAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR connection failed: {ex.Message}");
        }
    }

    private async Task StartLearning()
    {
        _showLearningDialog = true;
        _isLearning = true;
        _learningTimeLeft = 10;
        _learnedButton = null;

        _learningTimer = new System.Timers.Timer(1000);
        _learningTimer.Elapsed += OnLearningTimerElapsed;
        _learningTimer.Start();

        await LearnButton();
    }

    private async void OnLearningTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        _learningTimeLeft--;
        if (_learningTimeLeft <= 0)
        {
            _learningTimer?.Stop();
            _isLearning = false;
        }
        await InvokeAsync(StateHasChanged);
    }

    private void CancelLearning()
    {
        _learningTimer?.Stop();
        _isLearning = false;
        _showLearningDialog = false;
    }

    private async Task LearnButton()
    {
        try
        {
            var request = new { RemoteId, TimeoutSeconds = 10 };
            var response = await httpClient.PostAsJsonAsync($"/api/usb-remotes/{RemoteId}/learn-button", request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LearnButtonResponse>();
                if (result != null && result.Success)
                {
                    _learningTimer?.Stop();
                    _isLearning = false;
                    _learnedButton = new UsbRemoteButton
                    {
                        KeyCode = result.KeyCode,
                        ButtonName = result.ButtonName
                    };

                    await LoadData(); // Refresh buttons list
                }
            }
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to learn button: {ex.Message}";
            _learningTimer?.Stop();
            _isLearning = false;
            _showLearningDialog = false;
        }
    }

    private async Task LoadDeviceCommands(int deviceId)
    {
        try
        {
            // TODO: Load device commands from API
            // For now, create some default commands
            _deviceCommands = new List<DeviceCommand>
            {
                new DeviceCommand { Id = 1, Name = "power", DeviceId = deviceId },
                new DeviceCommand { Id = 2, Name = "volume_up", DeviceId = deviceId },
                new DeviceCommand { Id = 3, Name = "volume_down", DeviceId = deviceId },
                new DeviceCommand { Id = 4, Name = "mute", DeviceId = deviceId }
            };
            await InvokeAsync(StateHasChanged);
        }
        catch
        {
            _deviceCommands.Clear();
        }
    }

    private void AddMappingForButton(UsbRemoteButton button)
    {
        _newMapping = new CreateButtonMappingModel { ButtonId = button.Id };
        _showAddMappingDialog = true;
    }

    private async Task SaveMapping()
    {
        if (_mappingForm == null) return;

        await _mappingForm.Validate();
        if (!_mappingForm.IsValid) return;

        try
        {
            var response = await httpClient.PostAsJsonAsync("/api/usb-remotes/button-mappings", _newMapping);

            if (response.IsSuccessStatusCode)
            {
                _showAddMappingDialog = false;
                await LoadData();
            }
            else
            {
                _errorMessage = "Failed to save button mapping";
            }
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to save mapping: {ex.Message}";
        }
    }

    private void EditButton(UsbRemoteButton button)
    {
        // TODO: Implement button editing
    }

    private async Task DeleteButton(UsbRemoteButton button)
    {
        // TODO: Implement button deletion with confirmation
        await Task.CompletedTask;
    }

    private void ClearTestEvents()
    {
        _buttonEvents.Clear();
    }

    private string GetMappingDisplay(UsbRemoteButtonMapping mapping)
    {
        var device = _devices.FirstOrDefault(d => d.Id == mapping.DeviceId);
        return $"{device?.Name ?? "Unknown"} - {mapping.DeviceCommand?.Name ?? "Unknown"} ({mapping.EventType})";
    }

    private Color GetEventColor(string eventType)
    {
        return eventType switch
        {
            "KeyDown" => Color.Info,
            "KeyUp" => Color.Warning,
            "LongPress" => Color.Secondary,
            _ => Color.Primary
        };
    }

    public async ValueTask DisposeAsync()
    {
        _learningTimer?.Dispose();
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
    }

    private class CreateButtonMappingModel
    {
        public int ButtonId { get; set; }
        public int DeviceId { get; set; }
        public int DeviceCommandId { get; set; }
        public ButtonEventType EventType { get; set; } = ButtonEventType.KeyPress;
    }

    private class ButtonEventData
    {
        public string ButtonName { get; set; } = "";
        public string EventType { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }

    private class LearnButtonResponse
    {
        public bool Success { get; set; }
        public byte KeyCode { get; set; }
        public string ButtonName { get; set; } = "";
        public string Message { get; set; } = "";
    }

    private class UsbRemoteButtonEventData
    {
        public string DeviceId { get; set; } = "";
        public string RemoteName { get; set; } = "";
        public string ButtonName { get; set; } = "";
        public string EventType { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }
}