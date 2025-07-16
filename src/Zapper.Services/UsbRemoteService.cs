using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Zapper.Core.Models;
using Zapper.Data;
using Zapper.Device.USB;

namespace Zapper.Services;

public class UsbRemoteService : IUsbRemoteService
{
    private readonly ZapperContext _context;
    private readonly IUsbRemoteHandler _remoteHandler;
    private readonly IDeviceService _deviceService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<UsbRemoteService> _logger;

    public UsbRemoteService(
        ZapperContext context,
        IUsbRemoteHandler remoteHandler,
        IDeviceService deviceService,
        INotificationService notificationService,
        ILogger<UsbRemoteService> logger)
    {
        _context = context;
        _remoteHandler = remoteHandler;
        _deviceService = deviceService;
        _notificationService = notificationService;
        _logger = logger;

        // Wire up event handlers
        _remoteHandler.ButtonPressed += OnButtonPressed;
        _remoteHandler.RemoteConnected += OnRemoteConnected;
        _remoteHandler.RemoteDisconnected += OnRemoteDisconnected;
    }

    private async void OnButtonPressed(object? sender, RemoteButtonEventArgs e)
    {
        await ExecuteButtonMappingAsync(e.DeviceId, (byte)e.KeyCode, e.EventType);
    }

    private async void OnRemoteConnected(object? sender, string deviceId)
    {
        await HandleRemoteConnectedAsync(deviceId);
    }

    private async void OnRemoteDisconnected(object? sender, string deviceId)
    {
        await HandleRemoteDisconnectedAsync(deviceId);
    }
    public async Task<IEnumerable<UsbRemote>> GetAllRemotesAsync()
    {
        return await _context.UsbRemotes
            .Include(r => r.Buttons)
            .ToListAsync();
    }

    public async Task<UsbRemote?> GetRemoteByIdAsync(int id)
    {
        return await _context.UsbRemotes
            .Include(r => r.Buttons)
            .ThenInclude(b => b.Mappings)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<UsbRemote?> GetRemoteByDeviceIdAsync(string deviceId)
    {
        return await _context.UsbRemotes
            .Include(r => r.Buttons)
            .ThenInclude(b => b.Mappings)
            .FirstOrDefaultAsync(r => r.DeviceId == deviceId);
    }

    public async Task<UsbRemote> RegisterRemoteAsync(string deviceId, int vendorId, int productId, string? serialNumber = null, string? productName = null)
    {
        // Check if already registered
        var existing = await GetRemoteByDeviceIdAsync(deviceId);
        if (existing != null)
        {
            existing.LastSeenAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return existing;
        }

        var remote = new UsbRemote
        {
            DeviceId = deviceId,
            VendorId = vendorId,
            ProductId = productId,
            SerialNumber = serialNumber,
            ProductName = productName,
            Name = productName ?? $"Remote {vendorId:X4}:{productId:X4}",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            LastSeenAt = DateTime.UtcNow
        };

        _context.UsbRemotes.Add(remote);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Registered new USB remote: {Name} ({DeviceId})", remote.Name, deviceId);

        // Configure the handler
        _remoteHandler.ConfigureLongPressTimeout(deviceId, remote.LongPressTimeoutMs);
        _remoteHandler.ConfigureButtonInterception(deviceId, remote.InterceptSystemButtons);

        return remote;
    }

    public async Task UpdateRemoteAsync(int id, string name, bool isActive, bool interceptSystemButtons, int longPressTimeoutMs)
    {
        var remote = await _context.UsbRemotes.FindAsync(id);
        if (remote == null)
        {
            throw new ArgumentException($"Remote with ID {id} not found");
        }

        remote.Name = name;
        remote.IsActive = isActive;
        remote.InterceptSystemButtons = interceptSystemButtons;
        remote.LongPressTimeoutMs = longPressTimeoutMs;

        await _context.SaveChangesAsync();

        // Update handler configuration
        _remoteHandler.ConfigureLongPressTimeout(remote.DeviceId, longPressTimeoutMs);
        _remoteHandler.ConfigureButtonInterception(remote.DeviceId, interceptSystemButtons);

        _logger.LogInformation("Updated USB remote: {Name} ({DeviceId})", name, remote.DeviceId);
    }

    public async Task DeleteRemoteAsync(int id)
    {
        var remote = await _context.UsbRemotes.FindAsync(id);
        if (remote != null)
        {
            _context.UsbRemotes.Remove(remote);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Deleted USB remote: {Name} ({DeviceId})", remote.Name, remote.DeviceId);
        }
    }

    public async Task<IEnumerable<UsbRemoteButton>> GetRemoteButtonsAsync(int remoteId)
    {
        return await _context.UsbRemoteButtons
            .Where(b => b.RemoteId == remoteId)
            .Include(b => b.Mappings)
            .ToListAsync();
    }

    public async Task<UsbRemoteButton?> GetButtonByKeyCodeAsync(int remoteId, byte keyCode)
    {
        return await _context.UsbRemoteButtons
            .Include(b => b.Mappings)
            .FirstOrDefaultAsync(b => b.RemoteId == remoteId && b.KeyCode == keyCode);
    }

    public async Task<UsbRemoteButton> RegisterButtonAsync(int remoteId, byte keyCode, string buttonName, string? description = null)
    {
        // Check if already exists
        var existing = await GetButtonByKeyCodeAsync(remoteId, keyCode);
        if (existing != null)
        {
            return existing;
        }

        var button = new UsbRemoteButton
        {
            RemoteId = remoteId,
            KeyCode = keyCode,
            ButtonName = buttonName,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };

        _context.UsbRemoteButtons.Add(button);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Registered button {ButtonName} (0x{KeyCode:X2}) for remote {RemoteId}",
            buttonName, keyCode, remoteId);

        return button;
    }

    public async Task UpdateButtonAsync(int buttonId, string buttonName, string? description, bool allowInterception)
    {
        var button = await _context.UsbRemoteButtons.FindAsync(buttonId);
        if (button == null)
        {
            throw new ArgumentException($"Button with ID {buttonId} not found");
        }

        button.ButtonName = buttonName;
        button.Description = description;
        button.AllowInterception = allowInterception;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteButtonAsync(int buttonId)
    {
        var button = await _context.UsbRemoteButtons.FindAsync(buttonId);
        if (button != null)
        {
            _context.UsbRemoteButtons.Remove(button);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<UsbRemoteButtonMapping>> GetButtonMappingsAsync(int buttonId)
    {
        return await _context.UsbRemoteButtonMappings
            .Where(m => m.ButtonId == buttonId)
            .Include(m => m.Device)
            .Include(m => m.DeviceCommand)
            .ToListAsync();
    }

    public async Task<UsbRemoteButtonMapping> CreateButtonMappingAsync(int buttonId, int deviceId, int commandId, ButtonEventType eventType)
    {
        var mapping = new UsbRemoteButtonMapping
        {
            ButtonId = buttonId,
            DeviceId = deviceId,
            DeviceCommandId = commandId,
            EventType = eventType,
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.UsbRemoteButtonMappings.Add(mapping);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created button mapping: Button {ButtonId} -> Device {DeviceId}, Command {CommandId}, Event {EventType}",
            buttonId, deviceId, commandId, eventType);

        return mapping;
    }

    public async Task UpdateButtonMappingAsync(int mappingId, bool isEnabled, int priority)
    {
        var mapping = await _context.UsbRemoteButtonMappings.FindAsync(mappingId);
        if (mapping == null)
        {
            throw new ArgumentException($"Mapping with ID {mappingId} not found");
        }

        mapping.IsEnabled = isEnabled;
        mapping.Priority = priority;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteButtonMappingAsync(int mappingId)
    {
        var mapping = await _context.UsbRemoteButtonMappings.FindAsync(mappingId);
        if (mapping != null)
        {
            _context.UsbRemoteButtonMappings.Remove(mapping);
            await _context.SaveChangesAsync();
        }
    }

    public async Task ExecuteButtonMappingAsync(string deviceId, byte keyCode, ButtonEventType eventType)
    {
        var remote = await GetRemoteByDeviceIdAsync(deviceId);
        if (remote == null || !remote.IsActive)
        {
            _logger.LogWarning("Attempted to execute mapping for unregistered or inactive remote: {DeviceId}", deviceId);
            return;
        }

        var button = await GetButtonByKeyCodeAsync(remote.Id, keyCode);
        if (button == null)
        {
            _logger.LogDebug("No button registered for key code 0x{KeyCode:X2} on remote {RemoteId}", keyCode, remote.Id);
            return;
        }

        var mappings = await _context.UsbRemoteButtonMappings
            .Where(m => m.ButtonId == button.Id && m.EventType == eventType && m.IsEnabled)
            .Include(m => m.Device)
            .Include(m => m.DeviceCommand)
            .OrderByDescending(m => m.Priority)
            .ToListAsync();

        foreach (var mapping in mappings)
        {
            try
            {
                await _deviceService.SendCommand(mapping.DeviceId, mapping.DeviceCommand.Name);

                await _notificationService.NotifyDeviceCommandExecuted(
                    mapping.DeviceId,
                    mapping.Device.Name,
                    mapping.DeviceCommand.Name,
                    true);

                _logger.LogInformation("Executed command {Command} on device {Device} via USB remote button {Button}",
                    mapping.DeviceCommand.Name, mapping.Device.Name, button.ButtonName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute command {Command} on device {Device}",
                    mapping.DeviceCommand.Name, mapping.Device.Name);

                await _notificationService.NotifyDeviceCommandExecuted(
                    mapping.DeviceId,
                    mapping.Device.Name,
                    mapping.DeviceCommand.Name,
                    false);
            }
        }
    }

    public async Task HandleRemoteConnectedAsync(string deviceId)
    {
        try
        {
            // Parse device ID (format: VVVV:PPPP:serial)
            var parts = deviceId.Split(':');
            if (parts.Length < 2)
            {
                _logger.LogWarning("Invalid device ID format: {DeviceId}", deviceId);
                return;
            }

            var vendorId = Convert.ToInt32(parts[0], 16);
            var productId = Convert.ToInt32(parts[1], 16);
            var serialNumber = parts.Length > 2 ? parts[2] : null;

            // Get product name from connected devices
            var connectedRemotes = _remoteHandler.GetConnectedRemotes();
            var productName = connectedRemotes.FirstOrDefault() ?? "Unknown Remote";

            // Register or update the remote
            var remote = await RegisterRemoteAsync(deviceId, vendorId, productId, serialNumber, productName);

            // Send notification
            await _notificationService.SendSystemMessage(
                $"USB remote connected: {remote.Name}",
                "info");

            _logger.LogInformation("USB remote connected and registered: {Name} ({DeviceId})", remote.Name, deviceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle remote connection for device {DeviceId}", deviceId);
        }
    }

    public async Task HandleRemoteDisconnectedAsync(string deviceId)
    {
        try
        {
            var remote = await GetRemoteByDeviceIdAsync(deviceId);
            if (remote != null)
            {
                await _notificationService.SendSystemMessage(
                    $"USB remote disconnected: {remote.Name}",
                    "warning");

                _logger.LogInformation("USB remote disconnected: {Name} ({DeviceId})", remote.Name, deviceId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle remote disconnection for device {DeviceId}", deviceId);
        }
    }
}