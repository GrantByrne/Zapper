namespace Zapper.Core
{
    public class SupportedDevice
    {
        public const string WebOs = "WebOS";
        public const string Ir = "Infrared (IR)";
        public const string Bluetooth = "Bluetooth";

        public string[] GetAll()
        {
            return new[]
            {
                WebOs,
                Ir,
                Bluetooth
            };
        }
    }
}