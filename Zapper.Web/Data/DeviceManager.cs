using System;
using System.Collections.Generic;
using System.Linq;
using Zapper.Core;
using Zapper.Core.WebOs;
using Zapper.Web.Data.Abstract;

namespace Zapper.Web.Data
{
    public class DeviceManager : IDeviceManager
    {
        private const string Path = "devices.json";
        
        private readonly IWebOsActions _webOsActions;
        private readonly IFileSerializerConnection _fileSerializerConnection;
        private readonly List<Device> _devices;

        public DeviceManager(
            IWebOsActions webOsActions,
            IFileSerializerConnection fileSerializerConnection)
        {
            _webOsActions = webOsActions;
            _fileSerializerConnection = fileSerializerConnection;

            _devices = _fileSerializerConnection.Read<List<Device>>(Path) ?? new List<Device>();
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
            _fileSerializerConnection.Write(_devices, Path);
        }

        public void CreateWebOsDevice(string name, string ipAddress)
        {
            var device = new Device
            {
               Id = Guid.NewGuid(),
               Name = name,
               AvailableActions = _webOsActions
                   .GetAll()
                   .Select(a => new DeviceAction {Action = a})
                   .ToList(),
               IpAddress = ipAddress,
               SupportDeviceType = SupportedDevice.WebOs
            };
            
            _devices.Add(device);
            
            _fileSerializerConnection.Write(_devices, Path);
        }
    }
}