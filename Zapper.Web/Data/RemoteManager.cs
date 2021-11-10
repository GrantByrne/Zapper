using System.Collections.Generic;
using System.Linq;
using Zapper.Core;
using Zapper.Core.WebOs;
using Zapper.Core.WebOs.Abstract;
using Zapper.Web.Data.Abstract;

namespace Zapper.Web.Data
{
    public class RemoteManager : IRemoteManager
    {
        private const string Path = "remotes.json";
        
        private readonly IRemoteEventHandler _remoteEventHandler;
        private readonly IWebOsActions _webOsActions;
        private readonly IFileSerializerConnection _fileSerializerConnection;

        private List<RemoteButton> _cache;

        public RemoteManager(
            IRemoteEventHandler remoteEventHandler,
            IWebOsActions webOsActions,
            IFileSerializerConnection fileSerializerConnection)
        {
            _remoteEventHandler = remoteEventHandler;
            _webOsActions = webOsActions;
            _fileSerializerConnection = fileSerializerConnection;
        }

        public void Initialize()
        {
            _cache = _fileSerializerConnection.Read<List<RemoteButton>>(Path) ?? new List<RemoteButton>();
            
            var validButtons = _cache.Where(b => !string.IsNullOrEmpty(b.Action));
            foreach (var remote in validButtons)
            {
                var a = _webOsActions.Get(remote.Action);
                _remoteEventHandler.RegisterAction(remote.Code, a);   
            }
        }

        public IEnumerable<RemoteButton> Get()
        {
            return _cache;
        }

        public void Update(IEnumerable<RemoteButton> remoteButtons)
        {
            _remoteEventHandler.ClearActions();

            var validButtons = remoteButtons.Where(b => !string.IsNullOrEmpty(b.Action));
            foreach (var remote in validButtons)
            {
                var a = _webOsActions.Get(remote.Action);
                _remoteEventHandler.RegisterAction(remote.Code, a);   
            }
            
            _cache = remoteButtons.ToList();
            
            _fileSerializerConnection.Write(_cache, Path);
        }
    }
}