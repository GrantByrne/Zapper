using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Zapper.Core.Devices.Abstract;
using Zapper.Core.Repository;
using Zapper.Core.WebOs;
using Zapper.Core.WebOs.Abstract;

namespace Zapper.Core.Devices
{
    public class DeviceManager : IDeviceManager
    {
        private const string Filename = "devices.json";
        
        private readonly IFileSerializerConnection _fileSerializerConnection;
        private readonly IWebOsStatusManager _webOsStatusManager;
        private readonly IConfiguration _configuration;
        private readonly string _path;
        private List<Device> _devices;

        public DeviceManager(
            IFileSerializerConnection fileSerializerConnection,
            IWebOsStatusManager webOsStatusManager,
            IConfiguration configuration)
        {
            _fileSerializerConnection = fileSerializerConnection;
            _webOsStatusManager = webOsStatusManager;
            _configuration = configuration;

            _path = GetPath();
            
            Initialize();
        }

        public IEnumerable<Device> Get()
        {
            return _devices ?? new List<Device>();
        }

        public Device Get(Guid id)
        {
            return _devices.First(d => d.Id == id);
        }

        public void Delete(Guid id)
        {
            var device = _devices.FirstOrDefault(d => d.Id == id);

            if (device == null) 
                return;
            
            _devices.Remove(device);  
            _fileSerializerConnection.Write(_devices, _path);

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
            
            _fileSerializerConnection.Write(_devices, _path);
        }

        public void CreateWebOsDevice(string name, string ipAddress, string macAddress)
        {
            var device = new Device();
            
            device.Id = Guid.NewGuid();
            device.Name = name;
            
            device.AvailableActions = WebOsActionKey.All()
                .Select(a => new DeviceAction {
                    Action = a,
                    DeviceId = device.Id
                })
                .ToList();
            
            device.IpAddress = ipAddress;
            device.SupportDeviceType = SupportedDevice.WebOs;
            device.MacAddress = macAddress;

            _devices.Add(device);
            
            _fileSerializerConnection.Write(_devices, _path);
            _webOsStatusManager.Register(device.Id, ipAddress);
        }
        
        private void Initialize()
        {
            _devices = _fileSerializerConnection.Read<List<Device>>(_path) ?? new List<Device>();

            foreach (var device in _devices.Where(d => d.SupportDeviceType == SupportedDevice.WebOs))
            {
                device.AvailableActions = WebOsActionKey.All()
                    .Select(a => new DeviceAction {Action = a})
                    .ToList();
                
                _webOsStatusManager.Register(device.Id, device.IpAddress);
            }
        }

        private string GetPath()
        {
            var dir = _configuration["SettingsPath"];
            var p = Path.Combine(dir, Filename);
            return p;
        }
    }
}