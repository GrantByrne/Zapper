namespace Zapper.Contracts.UsbRemotes;

/// <summary>
/// Request to delete a button mapping.
/// </summary>
public class DeleteButtonMappingRequest
{
    /// <summary>
    /// Gets or sets the ID of the button mapping to delete.
    /// </summary>
    public int Id { get; set; }
}