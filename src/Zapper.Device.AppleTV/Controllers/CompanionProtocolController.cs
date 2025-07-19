using Microsoft.Extensions.Logging;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography;
using Zapper.Core.Models;
using Zapper.Device.AppleTV.Models;

namespace Zapper.Device.AppleTV.Controllers;

public class CompanionProtocolController(ILogger<CompanionProtocolController> logger)
    : BaseAppleTvController(logger)
{
    private TcpClient? _tcpClient;
    private SslStream? _sslStream;
    private byte[]? _sessionKey;

    public override ConnectionType SupportedProtocol => ConnectionType.CompanionProtocol;

    protected override async Task<bool> EstablishConnectionAsync(Zapper.Core.Models.Device device)
    {
        try
        {
            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(device.IpAddress!, device.Port ?? 49152);

            var networkStream = _tcpClient.GetStream();
            _sslStream = new SslStream(networkStream, false, ValidateServerCertificate);

            await _sslStream.AuthenticateAsClientAsync(device.IpAddress!);

            if (device is { RequiresPairing: true, IsPaired: false })
            {
                Logger.LogWarning("Device requires pairing. Please pair first.");
                return false;
            }

            if (device.PairingKey != null)
            {
                _sessionKey = DeriveSessionKey(device.PairingKey);
            }

            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to establish Companion Protocol connection");
            return false;
        }
    }

    protected override Task CloseConnectionAsync()
    {
        _sslStream?.Dispose();
        _tcpClient?.Dispose();
        _sessionKey = null;
        return Task.CompletedTask;
    }

    public override async Task<bool> SendCommandAsync(DeviceCommand command)
    {
        if (!IsConnected || _sslStream == null)
        {
            Logger.LogWarning("Not connected to Apple TV");
            return false;
        }

        try
        {
            var commandCode = MapDeviceCommand(command);
            if (commandCode == CommandCode.Unknown)
            {
                Logger.LogWarning("Unknown command: {Command}", command.Name);
                return false;
            }

            var packet = BuildCommandPacket(commandCode);
            if (_sessionKey != null)
            {
                packet = EncryptPacket(packet);
            }

            await _sslStream.WriteAsync(packet);
            await _sslStream.FlushAsync();

            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error sending command");
            return false;
        }
    }

    public override async Task<AppleTvStatus?> GetStatusAsync()
    {
        if (!IsConnected || _sslStream == null)
            return null;

        try
        {
            var statusRequest = BuildStatusRequestPacket();
            if (_sessionKey != null)
            {
                statusRequest = EncryptPacket(statusRequest);
            }

            await _sslStream.WriteAsync(statusRequest);
            await _sslStream.FlushAsync();

            var buffer = new byte[4096];
            var bytesRead = await _sslStream.ReadAsync(buffer);

            if (bytesRead > 0)
            {
                var response = buffer[..bytesRead];
                if (_sessionKey != null)
                {
                    response = DecryptPacket(response);
                }

                return ParseStatusResponse();
            }

            return null;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting status");
            return null;
        }
    }

    public override async Task<bool> PairAsync(string pin)
    {
        if (!IsConnected || _sslStream == null)
            return false;

        try
        {
            var pairingRequest = BuildPairingRequest(pin);
            await _sslStream.WriteAsync(pairingRequest);
            await _sslStream.FlushAsync();

            var buffer = new byte[1024];
            var bytesRead = await _sslStream.ReadAsync(buffer);

            if (bytesRead > 0)
            {
                var response = buffer[..bytesRead];
                var (success, pairingKey) = ParsePairingResponse(response);

                if (success && pairingKey != null && ConnectedDevice != null)
                {
                    ConnectedDevice.IsPaired = true;
                    ConnectedDevice.PairingKey = pairingKey;
                    _sessionKey = DeriveSessionKey(pairingKey);
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during pairing");
            return false;
        }
    }

    private bool ValidateServerCertificate(object sender, System.Security.Cryptography.X509Certificates.X509Certificate? certificate,
        System.Security.Cryptography.X509Certificates.X509Chain? chain, SslPolicyErrors sslPolicyErrors)
    {
        return true;
    }

    private byte[] DeriveSessionKey(byte[] pairingKey)
    {
        using var sha256 = SHA256.Create();
        return sha256.ComputeHash(pairingKey);
    }

    private byte[] BuildCommandPacket(CommandCode command)
    {
        var packet = new byte[8];
        packet[0] = 0x01;
        packet[1] = (byte)command;
        return packet;
    }

    private byte[] BuildStatusRequestPacket()
    {
        return [0x02, 0x00];
    }

    private byte[] BuildPairingRequest(string pin)
    {
        var pinBytes = System.Text.Encoding.UTF8.GetBytes(pin);
        var packet = new byte[pinBytes.Length + 2];
        packet[0] = 0x03;
        packet[1] = (byte)pinBytes.Length;
        Array.Copy(pinBytes, 0, packet, 2, pinBytes.Length);
        return packet;
    }

    private static byte[] EncryptPacket(byte[] data)
    {
        return data;
    }

    private static byte[] DecryptPacket(byte[] data)
    {
        return data;
    }

    private static AppleTvStatus ParseStatusResponse()
    {
        return new AppleTvStatus
        {
            PlaybackState = PlaybackState.Unknown
        };
    }

    private static (bool success, byte[]? pairingKey) ParsePairingResponse(byte[] response)
    {
        if (response.Length <= 0 || response[0] != 0x00 || response.Length <= 32) 
            return (false, null);

        var key = new byte[32];
        Array.Copy(response, 1, key, 0, 32);
            
        return (true, key);

    }
}