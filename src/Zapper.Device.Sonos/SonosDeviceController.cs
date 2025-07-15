using System.Collections.Concurrent;
using System.Text;
using System.Xml;
using Microsoft.Extensions.Logging;

namespace Zapper.Device.Sonos;

public class SonosDeviceController(HttpClient httpClient, ILogger<SonosDeviceController> logger) : ISonosDeviceController
{
    private readonly ConcurrentDictionary<string, SonosConnection> _connections = new();
    private const int SonosPort = 1400;

    public Task<bool> ConnectAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(device.IpAddress))
        {
            logger.LogWarning("Device {DeviceName} has no IP address configured", device.Name);
            return Task.FromResult(false);
        }

        try
        {
            if (_connections.ContainsKey(device.IpAddress))
            {
                logger.LogInformation("Already connected to Sonos at {IpAddress}", device.IpAddress);
                return Task.FromResult(true);
            }

            var connection = new SonosConnection
            {
                IpAddress = device.IpAddress,
                LastActivity = DateTime.UtcNow
            };

            _connections.TryAdd(device.IpAddress, connection);
            logger.LogInformation("Connected to Sonos at {IpAddress}", device.IpAddress);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to connect to Sonos at {IpAddress}", device.IpAddress);
            return Task.FromResult(false);
        }
    }

    public Task<bool> DisconnectAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(device.IpAddress))
            return Task.FromResult(false);

        _connections.TryRemove(device.IpAddress, out _);
        logger.LogInformation("Disconnected from Sonos at {IpAddress}", device.IpAddress);
        return Task.FromResult(true);
    }

    public async Task<bool> SendCommandAsync(Zapper.Core.Models.Device device, Core.Models.DeviceCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(device.IpAddress))
        {
            logger.LogWarning("Device {DeviceName} has no IP address configured", device.Name);
            return false;
        }

        try
        {
            return command.Type switch
            {
                Core.Models.CommandType.Power => await HandlePowerCommand(device, cancellationToken),
                Core.Models.CommandType.PlayPause => await HandlePlayPauseCommand(device, cancellationToken),
                Core.Models.CommandType.Stop => await StopAsync(device, cancellationToken),
                Core.Models.CommandType.VolumeUp => await AdjustVolumeAsync(device, 5, cancellationToken),
                Core.Models.CommandType.VolumeDown => await AdjustVolumeAsync(device, -5, cancellationToken),
                Core.Models.CommandType.Mute => await MuteAsync(device, cancellationToken),
                _ => await HandleUnknownCommand(command, cancellationToken)
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send command {CommandType} to Sonos device {DeviceName}", command.Type, device.Name);
            return false;
        }
    }

    public async Task<bool> TestConnectionAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(device.IpAddress))
            return false;

        try
        {
            var url = $"http://{device.IpAddress}:{SonosPort}/xml/device_description.xml";
            var response = await httpClient.GetAsync(url, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to test connection to Sonos device {DeviceName}", device.Name);
            return false;
        }
    }

    public async Task<bool> PowerOnAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default)
    {
        return await PlayAsync(device, cancellationToken);
    }

    public async Task<bool> PowerOffAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default)
    {
        return await PauseAsync(device, cancellationToken);
    }

    public async Task<bool> SetVolumeAsync(Zapper.Core.Models.Device device, int volume, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(device.IpAddress))
            return false;

        try
        {
            var soapAction = "\"urn:schemas-upnp-org:service:RenderingControl:1#SetVolume\"";
            var soapBody = $@"
                <?xml version=""1.0""?>
                <s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"" s:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"">
                    <s:Body>
                        <u:SetVolume xmlns:u=""urn:schemas-upnp-org:service:RenderingControl:1"">
                            <InstanceID>0</InstanceID>
                            <Channel>Master</Channel>
                            <DesiredVolume>{Math.Clamp(volume, 0, 100)}</DesiredVolume>
                        </u:SetVolume>
                    </s:Body>
                </s:Envelope>";

            return await SendSoapCommandAsync(device.IpAddress, "/MediaRenderer/RenderingControl/Control", soapAction, soapBody, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to set volume on Sonos device {DeviceName}", device.Name);
            return false;
        }
    }

    public async Task<bool> PlayAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(device.IpAddress))
            return false;

        try
        {
            var soapAction = "\"urn:schemas-upnp-org:service:AVTransport:1#Play\"";
            var soapBody = @"
                <?xml version=""1.0""?>
                <s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"" s:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"">
                    <s:Body>
                        <u:Play xmlns:u=""urn:schemas-upnp-org:service:AVTransport:1"">
                            <InstanceID>0</InstanceID>
                            <Speed>1</Speed>
                        </u:Play>
                    </s:Body>
                </s:Envelope>";

            return await SendSoapCommandAsync(device.IpAddress, "/MediaRenderer/AVTransport/Control", soapAction, soapBody, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to play on Sonos device {DeviceName}", device.Name);
            return false;
        }
    }

    public async Task<bool> PauseAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(device.IpAddress))
            return false;

        try
        {
            var soapAction = "\"urn:schemas-upnp-org:service:AVTransport:1#Pause\"";
            var soapBody = @"
                <?xml version=""1.0""?>
                <s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"" s:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"">
                    <s:Body>
                        <u:Pause xmlns:u=""urn:schemas-upnp-org:service:AVTransport:1"">
                            <InstanceID>0</InstanceID>
                        </u:Pause>
                    </s:Body>
                </s:Envelope>";

            return await SendSoapCommandAsync(device.IpAddress, "/MediaRenderer/AVTransport/Control", soapAction, soapBody, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to pause on Sonos device {DeviceName}", device.Name);
            return false;
        }
    }

    public async Task<bool> StopAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(device.IpAddress))
            return false;

        try
        {
            var soapAction = "\"urn:schemas-upnp-org:service:AVTransport:1#Stop\"";
            var soapBody = @"
                <?xml version=""1.0""?>
                <s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"" s:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"">
                    <s:Body>
                        <u:Stop xmlns:u=""urn:schemas-upnp-org:service:AVTransport:1"">
                            <InstanceID>0</InstanceID>
                        </u:Stop>
                    </s:Body>
                </s:Envelope>";

            return await SendSoapCommandAsync(device.IpAddress, "/MediaRenderer/AVTransport/Control", soapAction, soapBody, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to stop on Sonos device {DeviceName}", device.Name);
            return false;
        }
    }

    private async Task<bool> SendSoapCommandAsync(string ipAddress, string controlUrl, string soapAction, string soapBody, CancellationToken cancellationToken)
    {
        try
        {
            var url = $"http://{ipAddress}:{SonosPort}{controlUrl}";
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("SOAPAction", soapAction);
            request.Content = new StringContent(soapBody, Encoding.UTF8, "text/xml");

            var response = await httpClient.SendAsync(request, cancellationToken);
            logger.LogDebug("Sent SOAP command to Sonos at {IpAddress}: {Action}", ipAddress, soapAction);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send SOAP command to Sonos at {IpAddress}", ipAddress);
            return false;
        }
    }

    private async Task<bool> HandlePowerCommand(Zapper.Core.Models.Device device, CancellationToken cancellationToken)
    {
        var isPlaying = await IsPlayingAsync(device, cancellationToken);

        if (isPlaying)
        {
            return await PauseAsync(device, cancellationToken);
        }
        else
        {
            return await PlayAsync(device, cancellationToken);
        }
    }

    private async Task<bool> HandlePlayPauseCommand(Zapper.Core.Models.Device device, CancellationToken cancellationToken)
    {
        var isPlaying = await IsPlayingAsync(device, cancellationToken);

        if (isPlaying)
        {
            return await PauseAsync(device, cancellationToken);
        }
        else
        {
            return await PlayAsync(device, cancellationToken);
        }
    }

    private async Task<bool> IsPlayingAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(device.IpAddress))
            return false;

        try
        {
            var soapAction = "\"urn:schemas-upnp-org:service:AVTransport:1#GetTransportInfo\"";
            var soapBody = @"
                <?xml version=""1.0""?>
                <s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"" s:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"">
                    <s:Body>
                        <u:GetTransportInfo xmlns:u=""urn:schemas-upnp-org:service:AVTransport:1"">
                            <InstanceID>0</InstanceID>
                        </u:GetTransportInfo>
                    </s:Body>
                </s:Envelope>";

            var url = $"http://{device.IpAddress}:{SonosPort}/MediaRenderer/AVTransport/Control";
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("SOAPAction", soapAction);
            request.Content = new StringContent(soapBody, Encoding.UTF8, "text/xml");

            var response = await httpClient.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                return responseContent.Contains("PLAYING");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get transport info from Sonos device {DeviceName}", device.Name);
        }

        return false;
    }

    private async Task<bool> AdjustVolumeAsync(Zapper.Core.Models.Device device, int adjustment, CancellationToken cancellationToken)
    {
        var currentVolume = await GetCurrentVolumeAsync(device, cancellationToken);
        var newVolume = Math.Clamp(currentVolume + adjustment, 0, 100);
        return await SetVolumeAsync(device, newVolume, cancellationToken);
    }

    private async Task<int> GetCurrentVolumeAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(device.IpAddress))
            return 0;

        try
        {
            var soapAction = "\"urn:schemas-upnp-org:service:RenderingControl:1#GetVolume\"";
            var soapBody = @"
                <?xml version=""1.0""?>
                <s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"" s:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"">
                    <s:Body>
                        <u:GetVolume xmlns:u=""urn:schemas-upnp-org:service:RenderingControl:1"">
                            <InstanceID>0</InstanceID>
                            <Channel>Master</Channel>
                        </u:GetVolume>
                    </s:Body>
                </s:Envelope>";

            var url = $"http://{device.IpAddress}:{SonosPort}/MediaRenderer/RenderingControl/Control";
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("SOAPAction", soapAction);
            request.Content = new StringContent(soapBody, Encoding.UTF8, "text/xml");

            var response = await httpClient.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var doc = new XmlDocument();
                doc.LoadXml(responseContent);
                var volumeNode = doc.GetElementsByTagName("CurrentVolume").Item(0);
                if (volumeNode != null && int.TryParse(volumeNode.InnerText, out var volume))
                {
                    return volume;
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get current volume from Sonos device {DeviceName}", device.Name);
        }

        return 50;
    }

    private async Task<bool> MuteAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(device.IpAddress))
            return false;

        try
        {
            var soapAction = "\"urn:schemas-upnp-org:service:RenderingControl:1#SetMute\"";
            var soapBody = @"
                <?xml version=""1.0""?>
                <s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"" s:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"">
                    <s:Body>
                        <u:SetMute xmlns:u=""urn:schemas-upnp-org:service:RenderingControl:1"">
                            <InstanceID>0</InstanceID>
                            <Channel>Master</Channel>
                            <DesiredMute>1</DesiredMute>
                        </u:SetMute>
                    </s:Body>
                </s:Envelope>";

            return await SendSoapCommandAsync(device.IpAddress, "/MediaRenderer/RenderingControl/Control", soapAction, soapBody, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to mute Sonos device {DeviceName}", device.Name);
            return false;
        }
    }

    private Task<bool> HandleUnknownCommand(Core.Models.DeviceCommand command, CancellationToken cancellationToken)
    {
        logger.LogWarning("Unknown Sonos command type: {CommandType}", command.Type);
        return Task.FromResult(false);
    }

    private class SonosConnection
    {
        public string IpAddress { get; set; } = string.Empty;
        public DateTime LastActivity { get; set; }
    }
}