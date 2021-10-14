using System;
using System.Linq;

namespace Zapper.Web.Data
{
    public class DeviceManager
    {
        private readonly Device[] _devices;
        
        public DeviceManager()
        {
            _devices = BuildDevices();
        }
        
        public Device[] Get()
        {
            return _devices;
        }

        public Device Get(Guid id)
        {
            return _devices.First(d => d.Id == id);
        }

        private static Device[] BuildDevices()
        {
            var d1 = new Device
            {
                Id = Guid.NewGuid(),
                Name = "LG WebOS TV",
                On = true
            };

            var d2 = new Device
            {
                Id = Guid.NewGuid(),
                Name = "SMFL Amplifier",
                On = true
            };

            var d3 = new Device
            {
                Id = Guid.NewGuid(),
                Name = "Nvidia Shield",
                On = true
            };

            return new[] {d1, d2, d3};
        }
    }
}