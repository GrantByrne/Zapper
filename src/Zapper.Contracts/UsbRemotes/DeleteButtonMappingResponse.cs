namespace Zapper.Contracts.UsbRemotes;

/// <summary>
/// Response from deleting a button mapping.
/// </summary>
public class DeleteButtonMappingResponse
{
    /// <summary>
    /// Gets or sets whether the deletion was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets a message describing the result.
    /// </summary>
    public string Message { get; set; } = "";
}