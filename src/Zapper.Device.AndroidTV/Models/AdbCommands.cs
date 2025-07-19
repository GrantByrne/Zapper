namespace Zapper.Device.AndroidTV.Models;

public static class AdbCommands
{
    public const uint Connect = 0x4e584e43; // CNXN
    public const uint Auth = 0x48545541; // AUTH
    public const uint Open = 0x4e45504f; // OPEN
    public const uint Okay = 0x59414b4f; // OKAY
    public const uint Close = 0x45534c43; // CLSE
    public const uint Write = 0x45545257; // WRTE
    public const uint Sync = 0x434e5953; // SYNC
}