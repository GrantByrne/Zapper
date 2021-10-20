using System.Collections.Generic;
using System.Linq;
using Zapper.Web.Data.Abstract;

namespace Zapper.Web.Data
{
    public class RemoteManager : IRemoteManager
    {
        private List<RemoteButton> _cache = new();

        public IEnumerable<RemoteButton> Get()
        {
            return _cache;
        }

        public void Update(IEnumerable<RemoteButton> remoteButtons)
        {
            _cache = remoteButtons.ToList();
        }
    }
}