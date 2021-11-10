using System;

namespace Zapper.Core.WebOs.Abstract
{
    public interface IWebOsStatusManager
    {
        void Register(Guid id, string ipAddress);
        
        void DeRegister(Guid id);
        
        WebOsStatus GetStatus(Guid id);
    }
}