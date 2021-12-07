using System.Collections.Generic;
using Zapper.Core.Remote;

namespace Zapper.Web.Data.Abstract
{
    public interface IRemoteManager
    {
        IEnumerable<RemoteButton> Get();
        
        void Update(IEnumerable<RemoteButton> remoteButtons);

        void Initialize();
    }
}