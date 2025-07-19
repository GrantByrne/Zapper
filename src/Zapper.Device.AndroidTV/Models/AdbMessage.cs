namespace Zapper.Device.AndroidTV.Models;

public class AdbMessage
{
    public const uint AdbHeaderLength = 24;

    public uint Command { get; set; }
    public uint Arg0 { get; set; }
    public uint Arg1 { get; set; }
    public uint DataLength { get; set; }
    public uint DataCrc32 { get; set; }
    public uint Magic { get; set; }
    public byte[] Data { get; set; } = [];

    public byte[] ToBytes()
    {
        var buffer = new byte[AdbHeaderLength + DataLength];

        BitConverter.GetBytes(Command).CopyTo(buffer, 0);
        BitConverter.GetBytes(Arg0).CopyTo(buffer, 4);
        BitConverter.GetBytes(Arg1).CopyTo(buffer, 8);
        BitConverter.GetBytes(DataLength).CopyTo(buffer, 12);
        BitConverter.GetBytes(DataCrc32).CopyTo(buffer, 16);
        BitConverter.GetBytes(Magic).CopyTo(buffer, 20);

        if (Data.Length > 0)
        {
            Data.CopyTo(buffer, (int)AdbHeaderLength);
        }

        return buffer;
    }

    public static AdbMessage FromBytes(byte[] buffer)
    {
        if (buffer.Length < AdbHeaderLength)
            throw new ArgumentException("Buffer too small for ADB message header");

        var message = new AdbMessage
        {
            Command = BitConverter.ToUInt32(buffer, 0),
            Arg0 = BitConverter.ToUInt32(buffer, 4),
            Arg1 = BitConverter.ToUInt32(buffer, 8),
            DataLength = BitConverter.ToUInt32(buffer, 12),
            DataCrc32 = BitConverter.ToUInt32(buffer, 16),
            Magic = BitConverter.ToUInt32(buffer, 20)
        };

        if (message.DataLength > 0 && buffer.Length >= AdbHeaderLength + message.DataLength)
        {
            message.Data = new byte[message.DataLength];
            Array.Copy(buffer, (int)AdbHeaderLength, message.Data, 0, (int)message.DataLength);
        }

        return message;
    }
}