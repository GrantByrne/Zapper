using System.Collections.Generic;

namespace Zapper.Core
{
    public class LinuxDevice
    {
        public LinuxDeviceIdentifier Identifier { get; set; } = new();
        
        public string Name { get; set; }
        
        public string PhysicalPath { get; set; }
        
        public string SysFsPath { get; set; }
        
        public string UniqueIdentificationCode { get; set; }
        
        public List<string> Handlers { get; set; } = new();
        
        public List<string> Bitmaps { get; set; } = new();
    }
}