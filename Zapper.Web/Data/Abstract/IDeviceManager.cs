using System;

namespace Zapper.Web.Data.Abstract
{
    public interface IDeviceManager
    {
        Device[] Get();
        
        Device Get(Guid id);
    }
}