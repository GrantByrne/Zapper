namespace Zapper.Client.Remotes;

public class StartBluetoothAdvertisingRequest
{
    public string RemoteName { get; set; } = "Zapper Remote";
    public string DeviceType { get; set; } = "TVRemote"; // TVRemote, GameController, MediaRemote
}