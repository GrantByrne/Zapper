using System;
using System.Linq;
using Zapper.Core.WebOs;
using Zapper.Web.Data.Abstract;

namespace Zapper.Web.Data
{
    public class DeviceManager : IDeviceManager
    {
        private readonly IWebOsActions _webOsActions;
        private readonly Device[] _devices;

        public DeviceManager(IWebOsActions webOsActions)
        {
            _webOsActions = webOsActions;
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

        private Device[] BuildDevices()
        {
            var d1 = new Device
            {
                Id = Guid.NewGuid(),
                Name = "LG WebOS TV",
                On = true,
                AvailableActions = _webOsActions
                    .GetAll()
                    .Select(a => new DeviceAction {Action = a})
                    .ToList()
            };

            var d2 = new Device
            {
                Id = Guid.NewGuid(),
                Name = "SMFL Amplifier",
                On = true,
                AvailableActions = new[]
                    {
                        "Volume Up",
                        "Volume Down",
                        "Change Source"
                    }
                    .Select(a => new DeviceAction {Action = a})
                    .ToList()
            };

            var d3 = new Device
            {
                Id = Guid.NewGuid(),
                Name = "Nvidia Shield",
                On = true,
                AvailableActions = new[]
                    {
                        "Up",
                        "Down",
                        "Left",
                        "Right",
                        "A",
                        "B",
                        "X",
                        "Y",
                        "Start"
                    }
                    .Select(a => new DeviceAction {Action = a})
                    .ToList()
            };

            return new[] {d1, d2, d3};
        }
    }
}