namespace Zapper.Contracts.Devices;

public class SendCommandRequest
{
    public string Command { get; set; } = string.Empty;
    public Dictionary<string, object>? Parameters { get; set; }
}