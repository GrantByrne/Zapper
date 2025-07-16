using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Zapper.Client;
using Zapper.Contracts.Devices;
using Zapper.Core.Models;
using CommandType = Zapper.Core.Models.CommandType;

namespace Zapper.Blazor.Pages;

public partial class Remote(IZapperApiClient? apiClient) : ComponentBase
{
    private List<DeviceDto> _devices = new();
    private int? _selectedDeviceId;
    private bool _isLoading = true;
    private string? _errorMessage;
    private string _keyboardInput = "";
    private List<string> _additionalCommands = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadDevices();
    }

    private async Task LoadDevices()
    {
        try
        {
            _isLoading = true;
            _errorMessage = null;

            if (apiClient == null)
            {
                _errorMessage = "API client not configured";
                return;
            }

            var devices = await apiClient.Devices.GetAllDevicesAsync();
            _devices = devices.ToList();

            if (_devices.Any())
            {
                _selectedDeviceId = _devices.First().Id;
            }
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to load devices: {ex.Message}";
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private async Task SendCommand(string command)
    {
        if (apiClient == null || !_selectedDeviceId.HasValue)
        {
            return;
        }

        try
        {
            var request = new SendCommandRequest { Command = command };
            await apiClient.Devices.SendCommandAsync(_selectedDeviceId.Value, request);
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to send command: {ex.Message}";
            StateHasChanged();
        }
    }

    private async Task HandleCommandSend(CommandType commandType)
    {
        var commandName = commandType.ToString().ToLower();
        await SendCommand(commandName);
    }

    private async Task HandleCustomCommandSend(string command)
    {
        await SendCommand(command);
    }

    private async Task HandleNumberCommandSend(int number)
    {
        await SendCommand($"number_{number}");
    }

    private async Task HandleMouseMove((double deltaX, double deltaY) movement)
    {
        if (apiClient == null || !_selectedDeviceId.HasValue)
        {
            return;
        }

        try
        {
            var request = new SendCommandRequest
            {
                Command = "mouse_move",
                MouseDeltaX = (int)movement.deltaX,
                MouseDeltaY = (int)movement.deltaY
            };
            await apiClient.Devices.SendCommandAsync(_selectedDeviceId.Value, request);
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to send mouse movement: {ex.Message}";
            StateHasChanged();
        }
    }

    private async Task HandleMouseClick()
    {
        await SendCommand("mouse_click");
    }

    private async Task HandleRightClick()
    {
        await SendCommand("mouse_right_click");
    }

    private async Task HandleKeyboardInput(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await SendKeyboardText();
        }
    }

    private async Task SendKeyboardText()
    {
        if (string.IsNullOrWhiteSpace(_keyboardInput))
        {
            return;
        }

        if (apiClient == null || !_selectedDeviceId.HasValue)
        {
            return;
        }

        try
        {
            var request = new SendCommandRequest
            {
                Command = "keyboard_input",
                KeyboardText = _keyboardInput
            };
            await apiClient.Devices.SendCommandAsync(_selectedDeviceId.Value, request);
            _keyboardInput = "";
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to send keyboard input: {ex.Message}";
            StateHasChanged();
        }
    }

    private bool IsCommandAvailableForDevice(CommandType commandType)
    {
        var device = GetSelectedDevice();
        if (device == null) return false;

        return commandType switch
        {
            CommandType.Power => true,
            CommandType.VolumeUp or CommandType.VolumeDown or CommandType.Mute =>
                device.Type == DeviceType.Television || device.Type == DeviceType.SmartTv ||
                device.Type == DeviceType.Receiver || device.Type == DeviceType.SoundBar,
            CommandType.ChannelUp or CommandType.ChannelDown =>
                device.Type == DeviceType.Television || device.Type == DeviceType.CableBox,
            CommandType.PlayPause or CommandType.Stop or CommandType.FastForward or CommandType.Rewind =>
                device.Type == DeviceType.StreamingDevice || device.Type == DeviceType.SmartTv ||
                device.Type == DeviceType.AppleTv || device.Type == DeviceType.BluRayPlayer ||
                device.Type == DeviceType.DvdPlayer,
            CommandType.DirectionalUp or CommandType.DirectionalDown or
            CommandType.DirectionalLeft or CommandType.DirectionalRight or
            CommandType.Ok or CommandType.Back or CommandType.Home or CommandType.Menu => true,
            CommandType.MouseMove or CommandType.MouseClick => device.SupportsMouseInput,
            CommandType.KeyboardInput => device.SupportsKeyboardInput,
            _ => true
        };
    }

    private string GetCommandDisplayName(CommandType commandType)
    {
        return commandType switch
        {
            CommandType.DirectionalUp => "Up",
            CommandType.DirectionalDown => "Down",
            CommandType.DirectionalLeft => "Left",
            CommandType.DirectionalRight => "Right",
            CommandType.Ok => "OK",
            CommandType.VolumeUp => "Volume Up",
            CommandType.VolumeDown => "Volume Down",
            CommandType.ChannelUp => "Channel Up",
            CommandType.ChannelDown => "Channel Down",
            CommandType.PlayPause => "Play/Pause",
            CommandType.FastForward => "Fast Forward",
            CommandType.AppLaunch => "App Launch",
            CommandType.MouseMove => "Mouse Move",
            CommandType.MouseClick => "Mouse Click",
            CommandType.KeyboardInput => "Keyboard Input",
            _ => commandType.ToString()
        };
    }

    private DeviceDto? GetSelectedDevice()
    {
        if (!_selectedDeviceId.HasValue)
        {
            return null;
        }
        return _devices.FirstOrDefault(d => d.Id == _selectedDeviceId.Value);
    }
}