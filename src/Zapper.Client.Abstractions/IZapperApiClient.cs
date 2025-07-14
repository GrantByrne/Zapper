namespace Zapper.Client.Abstractions;

/// <summary>
/// Main client interface for the Zapper API
/// </summary>
public interface IZapperApiClient
{
    /// <summary>
    /// Device management client
    /// </summary>
    IDeviceClient Devices { get; }
}