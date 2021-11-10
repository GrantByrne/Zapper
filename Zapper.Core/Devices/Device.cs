using System;
using System.Collections.Generic;

namespace Zapper.Core.Devices
{
    public class Device
    {
        public Guid Id { get; set; }
        
        public string Name { get; set; }
        
        public bool On { get; set; }
        
        public string IpAddress { get; set; }
        
        public string SupportDeviceType { get; set; }
        
        public string MacAddress { get; set; }
        
        public List<DeviceAction> AvailableActions = new();
    }
}