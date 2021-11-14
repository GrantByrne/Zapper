using System.Collections.Generic;
using System.Linq;
using Zapper.Core;
using Zapper.Core.Devices;
using Zapper.Core.Devices.Abstract;
using Zapper.Core.Remote;
using Zapper.Core.Repository;
using Zapper.Core.WebOs;
using Zapper.Core.WebOs.Abstract;
using Zapper.Web.Data.Abstract;

namespace Zapper.Web.Data
{
    public class RemoteManager : IRemoteManager
    {
        private const string Path = "remotes.json";
        
        private readonly IRemoteEventHandler _remoteEventHandler;
        private readonly IFileSerializerConnection _fileSerializerConnection;
        private readonly IWebOsActionFactory _webOsActionFactory;
        private readonly IDeviceManager _deviceManager;

        private List<RemoteButton> _cache;

        public RemoteManager(
            IRemoteEventHandler remoteEventHandler,
            IFileSerializerConnection fileSerializerConnection,
            IWebOsActionFactory webOsActionFactory,
            IDeviceManager deviceManager)
        {
            _remoteEventHandler = remoteEventHandler;
            _fileSerializerConnection = fileSerializerConnection;
            _webOsActionFactory = webOsActionFactory;
            _deviceManager = deviceManager;
        }

        public void Initialize()
        {
            _cache = _fileSerializerConnection.Read<List<RemoteButton>>(Path) ?? new List<RemoteButton>();
            
            var validButtons = _cache.Where(b => !string.IsNullOrEmpty(b.Action));
            foreach (var remote in validButtons)
            {
                var device = _deviceManager.Get(remote.DeviceId);
                var a = _webOsActionFactory.Build(remote.Action, device);
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
                var device = _deviceManager.Get(remote.DeviceId);
                var a = _webOsActionFactory.Build(remote.Action, device);
                _remoteEventHandler.RegisterAction(remote.Code, a);   
            }
            
            _cache = remoteButtons.ToList();
            
            _fileSerializerConnection.Write(_cache, Path);
        }
    }
}