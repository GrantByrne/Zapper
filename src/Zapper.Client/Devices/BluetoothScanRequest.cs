namespace Zapper.Client.Devices;

/// <summary>
/// Request parameters for initiating a Bluetooth device scan.
/// </summary>
public class BluetoothScanRequest
{
    /// <summary>
    /// Gets or sets the duration in seconds for how long the Bluetooth scan should run.
    /// Default value is 30 seconds.
    /// </summary>
    /// <value>The scan duration in seconds. Must be a positive value.</value>
    public int DurationSeconds { get; set; } = 30;
}