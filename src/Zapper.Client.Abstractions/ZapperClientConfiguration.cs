namespace Zapper.Client.Abstractions;

/// <summary>
/// Configuration options for the Zapper API client
/// </summary>
public class ZapperClientConfiguration
{
    /// <summary>
    /// Base URL for the Zapper API
    /// </summary>
    public string BaseUrl { get; set; } = "http://localhost:5000";

    /// <summary>
    /// Request timeout in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Number of retry attempts for failed requests
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// API key for authentication (if required)
    /// </summary>
    public string? ApiKey { get; set; }
}