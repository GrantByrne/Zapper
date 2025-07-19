using Refit;
using Zapper.Client.Devices;
// using Zapper.Client; // No need to reference own namespace

namespace Zapper.Client;

/// <summary>
/// Refit interface for device API endpoints
/// </summary>
public interface IDeviceApi
{
    /// <summary>
    /// Get all devices
    /// </summary>
    [Get(ApiRoutes.Devices.GetAll)]
    Task<IEnumerable<DeviceDto>> GetAllDevicesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a device by ID
    /// </summary>
    [Get(ApiRoutes.Devices.GetById)]
    Task<DeviceDto> GetDeviceAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new device
    /// </summary>
    [Post(ApiRoutes.Devices.Create)]
    Task<DeviceDto> CreateDeviceAsync([Body] CreateDeviceRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing device
    /// </summary>
    [Put(ApiRoutes.Devices.Update)]
    Task<DeviceDto> UpdateDeviceAsync(int id, [Body] UpdateDeviceRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a device
    /// </summary>
    [Delete(ApiRoutes.Devices.Delete)]
    Task DeleteDeviceAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send a command to a device
    /// </summary>
    [Post(ApiRoutes.Devices.SendCommand)]
    Task SendCommandAsync(int id, [Body] SendCommandRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Discover available Bluetooth devices
    /// </summary>
    [Get(ApiRoutes.Devices.BluetoothDiscovery)]
    Task<IEnumerable<string>> DiscoverBluetoothDevicesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Start Bluetooth device scanning with real-time updates
    /// </summary>
    [Post(ApiRoutes.Devices.BluetoothScan)]
    Task<BluetoothScanResponse> StartBluetoothScanAsync([Body] BluetoothScanRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Start WebOS TV scanning with real-time updates
    /// </summary>
    [Post(ApiRoutes.Devices.WebOsScan)]
    Task<WebOsScanResponse> StartWebOsScanAsync([Body] WebOsScanRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stop Bluetooth device scanning
    /// </summary>
    [Post(ApiRoutes.Devices.BluetoothScanStop)]
    Task<StopBluetoothScanResponse> StopBluetoothScanAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stop WebOS TV scanning
    /// </summary>
    [Post(ApiRoutes.Devices.WebOsScanStop)]
    Task<StopWebOsScanResponse> StopWebOsScanAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Discover available PlayStation devices on the network
    /// </summary>
    [Post(ApiRoutes.Devices.PlayStationDiscovery)]
    Task<IEnumerable<PlayStationDeviceDto>> DiscoverPlayStationDevicesAsync([Body] DiscoverPlayStationDevicesRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Discover available Xbox devices on the network
    /// </summary>
    [Post(ApiRoutes.Devices.XboxDiscovery)]
    Task<DiscoverXboxDevicesResponse> DiscoverXboxDevicesAsync([Body] DiscoverXboxDevicesRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Discover available Roku devices on the network
    /// </summary>
    [Post(ApiRoutes.Devices.DiscoverRoku)]
    Task<IEnumerable<RokuDeviceDto>> DiscoverRokuDevicesAsync([Body] DiscoverRokuDevicesRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Discover available Yamaha devices on the network
    /// </summary>
    [Post(ApiRoutes.Devices.YamahaDiscovery)]
    Task<IEnumerable<YamahaDeviceDto>> DiscoverYamahaDevicesAsync([Body] DiscoverYamahaDevicesRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Discover available Sonos devices on the network
    /// </summary>
    [Post(ApiRoutes.Devices.SonosDiscovery)]
    Task<IEnumerable<SonosDeviceDto>> DiscoverSonosDevicesAsync([Body] DiscoverSonosDevicesRequest request, CancellationToken cancellationToken = default);
}