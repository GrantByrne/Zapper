namespace Zapper.Endpoints.Devices;

public class SendCommandRequest
{
    public int Id { get; set; }
    public string CommandName { get; set; } = string.Empty;
}