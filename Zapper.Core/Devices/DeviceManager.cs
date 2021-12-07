using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zapper.Core.Devices.Abstract;
using Zapper.Core.Repository;
using Zapper.Core.WebOs;
using Zapper.Core.WebOs.Abstract;

namespace Zapper.Core.Devices
{
    public class DeviceManager : IDeviceManager
    {
        private const string Filename = "devices.json";

        private readonly IWebOsStatusManager _webOsStatusManager;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private List<Device> _devices;

        public DeviceManager(
            IWebOsStatusManager webOsStatusManager,
            IServiceScopeFactory serviceScopeFactory)
        {
            _webOsStatusManager = webOsStatusManager;
            _serviceScopeFactory = serviceScopeFactory;

            Initialize();
        }

        public IEnumerable<Device> Get()
        {
            return _devices ?? new List<Device>();
        }

        public Device Get(Guid id)
        {
            return _devices.FirstOrDefault(d => d.Id == id);
        }

        public void Delete(Guid id)
        {
            var device = _devices.FirstOrDefault(d => d.Id == id);

            if (device == null)
                return;

            _devices.Remove(device);

            using var scope = _serviceScopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ZapperDbContext>();
            db.Devices.Remove(device);
            db.SaveChanges();

            if (device.SupportDeviceType == SupportedDevice.WebOs)
            {
                _webOsStatusManager.DeRegister(device.Id);
            }
        }

        public void CreateIrDevice(string name)
        {
            var device = new Device
            {
                Id = Guid.NewGuid(),
                Name = name,
                SupportDeviceType = SupportedDevice.Ir
            };

            _devices.Add(device);

            using var scope = _serviceScopeFactory.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<ZapperDbContext>();
            db.Add(device);
            db.SaveChanges();
        }

        public void CreateWebOsDevice(string name, string ipAddress, string macAddress)
        {
            var device = new Device();

            device.Id = Guid.NewGuid();
            device.Name = name;

            device.AvailableActions = WebOsActionKey.All()
                .Select(a => new DeviceAction
                {
                    Action = a,
                    DeviceId = device.Id
                })
                .ToList();

            device.IpAddress = ipAddress;
            device.SupportDeviceType = SupportedDevice.WebOs;
            device.MacAddress = macAddress;

            _devices.Add(device);

            using var scope = _serviceScopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ZapperDbContext>();
            db.Add(device);
            db.SaveChanges();

            _webOsStatusManager.Register(device.Id, ipAddress);
        }

        private void Initialize()
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ZapperDbContext>();
            _devices = db.Devices.ToList();

            foreach (var device in _devices.Where(d => d.SupportDeviceType == SupportedDevice.WebOs))
            {
                device.AvailableActions = WebOsActionKey.All()
                    .Select(a => new DeviceAction {Action = a})
                    .ToList();

                _webOsStatusManager.Register(device.Id, device.IpAddress);
            }
        }
    }
}