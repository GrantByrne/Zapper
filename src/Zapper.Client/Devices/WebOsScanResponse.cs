namespace Zapper.Client.Devices;

/// <summary>
/// Response returned after initiating or checking the status of a WebOS device discovery scan.
/// </summary>
public class WebOsScanResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether the WebOS scan operation was successful.
    /// </summary>
    /// <value><c>true</c> if the operation succeeded; otherwise, <c>false</c>.</value>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets an optional message providing additional details about the scan operation.
    /// This may contain error information if the operation failed or discovery results.
    /// </summary>
    /// <value>A message describing the result of the operation, or <c>null</c> if no message is available.</value>
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a WebOS discovery scan is currently in progress.
    /// </summary>
    /// <value><c>true</c> if a scan is active; otherwise, <c>false</c>.</value>
    public bool IsScanning { get; set; }
}