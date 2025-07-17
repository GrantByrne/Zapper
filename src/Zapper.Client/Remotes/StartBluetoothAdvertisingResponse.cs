namespace Zapper.Client.Remotes;

public class StartBluetoothAdvertisingResponse
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsAdvertising { get; set; }
}