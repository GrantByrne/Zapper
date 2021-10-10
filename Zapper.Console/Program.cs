using System.Linq;
using System.Text;
using Zapper.Core;

namespace Zapper.Console
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using var lol = new Lol();

            lol.OnKeyPress += LolOnOnKeyPress;

            System.Console.ReadLine();

            lol.OnKeyPress -= LolOnOnKeyPress;
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