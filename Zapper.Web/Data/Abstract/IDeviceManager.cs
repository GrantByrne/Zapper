using System;
using System.Collections.Generic;

namespace Zapper.Web.Data.Abstract
{
    public interface IDeviceManager
    {
        IEnumerable<Device> Get();
        
        Device Get(Guid id);

        void CreateWebOsDevice(string name, string ipAddress, string macAddress);

        void CreateIrDevice(string name);

        void Delete(Guid id);
    }
}