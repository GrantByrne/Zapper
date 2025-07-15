// using Zapper.Client; // No need to reference own namespace
using Zapper.Contracts.Devices;

namespace Zapper.Client;

/// <summary>
/// Implementation of device client using Refit
/// </summary>
public class DeviceClient(IDeviceApi deviceApi) : IDeviceClient
{
    public async Task<IEnumerable<DeviceDto>> GetAllDevicesAsync(CancellationToken cancellationToken = default)
    {
        return await deviceApi.GetAllDevicesAsync(cancellationToken);
    }

    public async Task<DeviceDto?> GetDeviceAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await deviceApi.GetDeviceAsync(id, cancellationToken);
        }
        catch (Refit.ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<DeviceDto> CreateDeviceAsync(CreateDeviceRequest request, CancellationToken cancellationToken = default)
    {
        return await deviceApi.CreateDeviceAsync(request, cancellationToken);
    }

    public async Task<DeviceDto> UpdateDeviceAsync(int id, UpdateDeviceRequest request, CancellationToken cancellationToken = default)
    {
        return await deviceApi.UpdateDeviceAsync(id, request, cancellationToken);
    }

    public async Task DeleteDeviceAsync(int id, CancellationToken cancellationToken = default)
    {
        await deviceApi.DeleteDeviceAsync(id, cancellationToken);
    }

    public async Task SendCommandAsync(int id, SendCommandRequest request, CancellationToken cancellationToken = default)
    {
        await deviceApi.SendCommandAsync(id, request, cancellationToken);
    }

    public async Task<IEnumerable<string>> DiscoverBluetoothDevicesAsync(CancellationToken cancellationToken = default)
    {
        return await deviceApi.DiscoverBluetoothDevicesAsync(cancellationToken);
    }

    public async Task<BluetoothScanResponse> StartBluetoothScanAsync(BluetoothScanRequest request, CancellationToken cancellationToken = default)
    {
        return await deviceApi.StartBluetoothScanAsync(request, cancellationToken);
    }

    public async Task<WebOsScanResponse> StartWebOsScanAsync(WebOsScanRequest request, CancellationToken cancellationToken = default)
    {
        return await deviceApi.StartWebOsScanAsync(request, cancellationToken);
    }

    public async Task<StopBluetoothScanResponse> StopBluetoothScanAsync(CancellationToken cancellationToken = default)
    {
        return await deviceApi.StopBluetoothScanAsync(cancellationToken);
    }

    public async Task<StopWebOsScanResponse> StopWebOsScanAsync(CancellationToken cancellationToken = default)
    {
        return await deviceApi.StopWebOsScanAsync(cancellationToken);
    }

    public async Task<IEnumerable<PlayStationDeviceDto>> DiscoverPlayStationDevicesAsync(DiscoverPlayStationDevicesRequest request, CancellationToken cancellationToken = default)
    {
        return await deviceApi.DiscoverPlayStationDevicesAsync(request, cancellationToken);
    }

    public async Task<DiscoverXboxDevicesResponse> DiscoverXboxDevicesAsync(DiscoverXboxDevicesRequest request, CancellationToken cancellationToken = default)
    {
        return await deviceApi.DiscoverXboxDevicesAsync(request, cancellationToken);
    }
}