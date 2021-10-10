using System.Linq;
using System.Text;
using Zapper.Core;

namespace Zapper.Console
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var devices = DeviceManager.Get()
                .ToArray();

            for (var x = 0; x < devices.Length; x++)
            {
                var device = devices[x];
                System.Console.WriteLine($"{x}.");
                var details = DisplayDeviceDetails(device);
                System.Console.WriteLine(details);
                System.Console.WriteLine();
            }

            var choice = System.Console.ReadLine();

            if (!int.TryParse(choice, out var index))
            {
                return;
            }

            var d = devices[index];

            using var lol = new DeviceInputReader(d);
            
            lol.OnKeyPress += LolOnOnKeyPress;

            System.Console.ReadLine();
        }

        private static void LolOnOnKeyPress(KeyPressEvent e)
        {
            System.Console.WriteLine($"Code: {e.Code} State {e.State}");
        }

        private static string DisplayDeviceDetails(LinuxDevice linuxDevice)
        {
            var sb = new StringBuilder();

            sb.Append("Identifier { ");
            sb.Append($"Bus: {linuxDevice.Identifier.Bus} ");
            sb.Append($"Product: {linuxDevice.Identifier.Product} ");
            sb.Append($"Vendor: {linuxDevice.Identifier.Vendor} ");
            sb.Append($"Version: {linuxDevice.Identifier.Version} ");
            sb.Append("}\n");

            sb.AppendLine($"Name: {linuxDevice.Name}");
            sb.AppendLine($"Physical Path: {linuxDevice.PhysicalPath}");
            sb.AppendLine($"SysFsPath Path: {linuxDevice.SysFsPath}");
            sb.AppendLine($"UniqueIdentificationCode: {linuxDevice.UniqueIdentificationCode}");
            sb.AppendLine($"Handlers: {string.Join(", ", linuxDevice.Handlers)}");
            
            return sb.ToString();
        }
    }
}