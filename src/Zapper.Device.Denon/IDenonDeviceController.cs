namespace Zapper.Device.Denon;

public interface IDenonDeviceController
{
    Task<bool> SetPowerAsync(bool powerOn, CancellationToken cancellationToken = default);
    Task<bool> GetPowerStatusAsync(CancellationToken cancellationToken = default);
    Task<bool> SetVolumeAsync(int volume, CancellationToken cancellationToken = default);
    Task<int> GetVolumeAsync(CancellationToken cancellationToken = default);
    Task<bool> SetMuteAsync(bool mute, CancellationToken cancellationToken = default);
    Task<bool> GetMuteStatusAsync(CancellationToken cancellationToken = default);
    Task<bool> SetInputAsync(string input, CancellationToken cancellationToken = default);
    Task<string> GetCurrentInputAsync(CancellationToken cancellationToken = default);
    Task<bool> VolumeUpAsync(CancellationToken cancellationToken = default);
    Task<bool> VolumeDownAsync(CancellationToken cancellationToken = default);
    Task<Dictionary<string, string>> GetAvailableInputsAsync(CancellationToken cancellationToken = default);
    Task<string> GetModelInfoAsync(CancellationToken cancellationToken = default);
    Task<bool> SetZonePowerAsync(string zone, bool powerOn, CancellationToken cancellationToken = default);
    Task<bool> SetZoneVolumeAsync(string zone, int volume, CancellationToken cancellationToken = default);
    Task<bool> SetZoneInputAsync(string zone, string input, CancellationToken cancellationToken = default);
}