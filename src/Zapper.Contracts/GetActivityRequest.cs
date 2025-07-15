namespace Zapper.Contracts;

/// <summary>
/// Represents a request to retrieve a specific activity by its identifier.
/// </summary>
public class GetActivityRequest
{
    /// <summary>
    /// Gets or sets the unique identifier of the activity to retrieve.
    /// </summary>
    public int Id { get; set; }
}