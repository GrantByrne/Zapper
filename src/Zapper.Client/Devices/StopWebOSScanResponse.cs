namespace Zapper.Client.Devices;

/// <summary>
/// Represents the response from a request to stop a WebOS device scan.
/// Contains the result of the stop operation and any relevant messages.
/// </summary>
public class StopWebOsScanResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether the WebOS scan was successfully stopped.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets an optional message describing the result of the stop operation.
    /// This may contain success confirmation or error details.
    /// </summary>
    public string? Message { get; set; }
}