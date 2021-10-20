using System.Collections.Generic;

namespace Zapper.Web.Data.Abstract
{
    public interface IRemoteManager
    {
        IEnumerable<RemoteButton> Get();
        void Update(IEnumerable<RemoteButton> remoteButtons);
    }
}