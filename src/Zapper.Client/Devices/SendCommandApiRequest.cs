namespace Zapper.Client.Devices;

public class SendCommandApiRequest
{
    public int Id { get; set; }
    public string Command { get; set; } = "";
    public int? MouseDeltaX { get; set; }
    public int? MouseDeltaY { get; set; }
    public string? KeyboardText { get; set; }
    public Dictionary<string, object>? Parameters { get; set; }
}