using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Zapper.Core.Keyboard
{
    public static class DeviceManager
    {
        public static IEnumerable<LinuxDevice> Get(string path = "/proc/bus/input/devices")
        {
            var devices = new List<LinuxDevice>();

            using var filestream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(filestream);
            
            var linuxDevice = new LinuxDevice();
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();

                if (string.IsNullOrWhiteSpace(line))
                {
                    if (!string.IsNullOrWhiteSpace(linuxDevice.Name))
                    {
                        devices.Add(linuxDevice);
                        linuxDevice = new LinuxDevice();    
                    }
                    
                    continue;
                }

                if (line.StartsWith("I"))
                    ApplyIdentifier(line, linuxDevice);
                    
                else if (line.StartsWith("N")) 
                    linuxDevice.Name = line.Substring(9, line.Length - 9 - 1);
                    
                else if (line.StartsWith("P"))
                    linuxDevice.PhysicalPath = line[8..];
                
                else if (line.StartsWith("S")) 
                    linuxDevice.SysFsPath = line[9..];
                
                else if (line.StartsWith("U")) 
                    linuxDevice.UniqueIdentificationCode = line[8..];
                
                else if (line.StartsWith("H")) 
                    linuxDevice.Handlers = line[12..]
                        .Split(" ")
                        .Where(h => !string.IsNullOrWhiteSpace(h))
                        .ToList();
                
                else if (line.StartsWith("B"))
                    linuxDevice.Bitmaps.Add(line[3..]);
            }

            return devices;
        }

        private static void ApplyIdentifier(string line, LinuxDevice linuxDevice)
        {
            var values = line[3..]
                .Split(" ");

            foreach (var v in values)
            {
                var kvp = v.Split("=");

                switch (kvp[0])
                {
                    case "Bus":
                        linuxDevice.Identifier.Bus = kvp[1];
                        break;
                    case "Vendor":
                        linuxDevice.Identifier.Vendor = kvp[1];
                        break;
                    case "Product":
                        linuxDevice.Identifier.Product = kvp[1];
                        break;
                    case "Version":
                        linuxDevice.Identifier.Version = kvp[1];
                        break;
                }
            }
        }
    }
}