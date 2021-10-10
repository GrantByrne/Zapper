using Zapper.Core;

namespace Zapper.Console
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var devices = DeviceManager.Get();

            foreach (var device in devices)
            {
                System.Console.WriteLine(device.Name);
            }

            System.Console.ReadLine();
        }
    }
}