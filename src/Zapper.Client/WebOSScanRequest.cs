namespace Zapper.Client;

/// <summary>
/// Request parameters for initiating a WebOS device discovery scan.
/// </summary>
public class WebOsScanRequest
{
    /// <summary>
    /// Gets or sets the duration in seconds for how long the WebOS discovery scan should run.
    /// Default value is 15 seconds, which is typically sufficient for local network discovery.
    /// </summary>
    /// <value>The scan duration in seconds. Must be a positive value.</value>
    public int DurationSeconds { get; set; } = 15;
}