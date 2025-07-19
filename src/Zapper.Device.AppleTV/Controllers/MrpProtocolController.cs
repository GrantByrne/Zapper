using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using Zapper.Core.Models;
using Zapper.Device.AppleTV.Models;

namespace Zapper.Device.AppleTV.Controllers;

public class MrpProtocolController(ILogger<MrpProtocolController> logger)
    : BaseAppleTvController(logger)
{
    private TcpClient? _tcpClient;
    private NetworkStream? _networkStream;

    public override ConnectionType SupportedProtocol => ConnectionType.MediaRemoteProtocol;

    protected override async Task<bool> EstablishConnectionAsync(Zapper.Core.Models.Device device)
    {
        try
        {
            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(device.IpAddress!, device.Port ?? 7000);

            _networkStream = _tcpClient.GetStream();

            if (device.RequiresPairing && !device.IsPaired)
            {
                Logger.LogWarning("Device requires pairing. Please pair first.");
                return false;
            }

            await SendHandshakeAsync();

            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to establish MRP connection");
            return false;
        }
    }

    protected override Task CloseConnectionAsync()
    {
        _networkStream?.Dispose();
        _tcpClient?.Dispose();
        return Task.CompletedTask;
    }

    public override async Task<bool> SendCommandAsync(DeviceCommand command)
    {
        if (!IsConnected || _networkStream == null)
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

            var protobufMessage = CreateCommandMessage(commandCode);
            var packet = WrapInMrpPacket(protobufMessage);

            await _networkStream.WriteAsync(packet);
            await _networkStream.FlushAsync();

            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error sending MRP command");
            return false;
        }
    }

    public override async Task<AppleTvStatus?> GetStatusAsync()
    {
        if (!IsConnected || _networkStream == null)
            return null;

        try
        {
            var statusMessage = CreateStatusRequestMessage();
            var packet = WrapInMrpPacket(statusMessage);

            await _networkStream.WriteAsync(packet);
            await _networkStream.FlushAsync();

            var buffer = new byte[4096];
            var bytesRead = await _networkStream.ReadAsync(buffer);

            if (bytesRead > 0)
            {
                return ParseMrpStatusResponse(buffer[..bytesRead]);
            }

            return null;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting MRP status");
            return null;
        }
    }

    public override async Task<bool> PairAsync(string pin)
    {
        if (!IsConnected || _networkStream == null)
            return false;

        try
        {
            var pairingMessage = CreatePairingMessage(pin);
            var packet = WrapInMrpPacket(pairingMessage);

            await _networkStream.WriteAsync(packet);
            await _networkStream.FlushAsync();

            var buffer = new byte[1024];
            var bytesRead = await _networkStream.ReadAsync(buffer);

            if (bytesRead <= 0) 
                return false;
            
            var success = ParsePairingResponse(buffer[..bytesRead]);
            
            if (!success || ConnectedDevice == null) 
                return false;
            
            ConnectedDevice.IsPaired = true;
            return true;

        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during MRP pairing");
            return false;
        }
    }

    private async Task SendHandshakeAsync()
    {
        var handshake = CreateHandshakeMessage();
        var packet = WrapInMrpPacket(handshake);
        await _networkStream!.WriteAsync(packet);
        await _networkStream.FlushAsync();
    }

    private byte[] CreateHandshakeMessage()
    {
        return [0x08, 0x01, 0x10, 0x01];
    }

    private static byte[] CreateCommandMessage(CommandCode command)
    {
        var messageType = command switch
        {
            CommandCode.Play => 0x01,
            CommandCode.Pause => 0x02,
            CommandCode.Stop => 0x03,
            CommandCode.Next => 0x04,
            CommandCode.Previous => 0x05,
            CommandCode.FastForward => 0x06,
            CommandCode.Rewind => 0x07,
            CommandCode.VolumeUp => 0x08,
            CommandCode.VolumeDown => 0x09,
            CommandCode.Select => 0x0A,
            CommandCode.Menu => 0x0B,
            CommandCode.Home => 0x0C,
            CommandCode.Up => 0x0D,
            CommandCode.Down => 0x0E,
            CommandCode.Left => 0x0F,
            CommandCode.Right => 0x10,
            _ => 0x00
        };

        return [0x08, (byte)messageType, 0x10, 0x01];
    }

    private byte[] CreateStatusRequestMessage()
    {
        return [0x08, 0x20, 0x10, 0x01];
    }

    private byte[] CreatePairingMessage(string pin)
    {
        var pinBytes = System.Text.Encoding.UTF8.GetBytes(pin);
        var message = new byte[pinBytes.Length + 4];
        message[0] = 0x08;
        message[1] = 0x30;
        message[2] = 0x12;
        message[3] = (byte)pinBytes.Length;
        Array.Copy(pinBytes, 0, message, 4, pinBytes.Length);
        return message;
    }

    private byte[] WrapInMrpPacket(byte[] message)
    {
        var packet = new byte[message.Length + 4];
        var lengthBytes = BitConverter.GetBytes(message.Length);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(lengthBytes);

        Array.Copy(lengthBytes, 0, packet, 0, 4);
        Array.Copy(message, 0, packet, 4, message.Length);

        return packet;
    }

    private AppleTvStatus ParseMrpStatusResponse(byte[] response)
    {
        if (response.Length < 8)
            return new AppleTvStatus { PlaybackState = PlaybackState.Unknown };

        var playbackState = response[5] switch
        {
            0x01 => PlaybackState.Playing,
            0x02 => PlaybackState.Paused,
            0x03 => PlaybackState.Stopped,
            _ => PlaybackState.Unknown
        };

        return new AppleTvStatus
        {
            PlaybackState = playbackState,
            IsPlaying = playbackState == PlaybackState.Playing
        };
    }

    private bool ParsePairingResponse(byte[] response)
    {
        return response.Length > 4 && response[5] == 0x00;
    }
}