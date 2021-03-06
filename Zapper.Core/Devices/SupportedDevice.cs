namespace Zapper.Core.Devices
{
    public static class SupportedDevice
    {
        public const string WebOs = "WebOS";
        public const string Ir = "Infrared (IR)";
        public const string Bluetooth = "Bluetooth";

        public static string[] All()
        {
            return new[]
            {
                WebOs,
                Bluetooth
            };
        }
    }
}