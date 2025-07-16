using Zapper.Core.Models;

namespace Zapper.Services;

public interface IUsbRemoteService
{
    Task<IEnumerable<UsbRemote>> GetAllRemotesAsync();
    Task<UsbRemote?> GetRemoteByIdAsync(int id);
    Task<UsbRemote?> GetRemoteByDeviceIdAsync(string deviceId);
    Task<UsbRemote> RegisterRemoteAsync(string deviceId, int vendorId, int productId, string? serialNumber = null, string? productName = null);
    Task UpdateRemoteAsync(int id, string name, bool isActive, bool interceptSystemButtons, int longPressTimeoutMs);
    Task DeleteRemoteAsync(int id);

    Task<IEnumerable<UsbRemoteButton>> GetRemoteButtonsAsync(int remoteId);
    Task<UsbRemoteButton?> GetButtonByKeyCodeAsync(int remoteId, byte keyCode);
    Task<UsbRemoteButton> RegisterButtonAsync(int remoteId, byte keyCode, string buttonName, string? description = null);
    Task UpdateButtonAsync(int buttonId, string buttonName, string? description, bool allowInterception);
    Task DeleteButtonAsync(int buttonId);

    Task<IEnumerable<UsbRemoteButtonMapping>> GetButtonMappingsAsync(int buttonId);
    Task<UsbRemoteButtonMapping> CreateButtonMappingAsync(int buttonId, int deviceId, int commandId, ButtonEventType eventType);
    Task UpdateButtonMappingAsync(int mappingId, bool isEnabled, int priority);
    Task DeleteButtonMappingAsync(int mappingId);

    Task ExecuteButtonMappingAsync(string deviceId, byte keyCode, ButtonEventType eventType);
    Task HandleRemoteConnectedAsync(string deviceId);
    Task HandleRemoteDisconnectedAsync(string deviceId);
}