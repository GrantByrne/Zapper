using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
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
        private const string Filename = "remotes.json";
        
        private readonly IRemoteEventHandler _remoteEventHandler;
        private readonly IFileSerializerConnection _fileSerializerConnection;
        private readonly IWebOsActionFactory _webOsActionFactory;
        private readonly IDeviceManager _deviceManager;
        private readonly IConfiguration _configuration;

        private List<RemoteButton> _cache;

        public RemoteManager(
            IRemoteEventHandler remoteEventHandler,
            IFileSerializerConnection fileSerializerConnection,
            IWebOsActionFactory webOsActionFactory,
            IDeviceManager deviceManager,
            IConfiguration configuration)
        {
            _remoteEventHandler = remoteEventHandler;
            _fileSerializerConnection = fileSerializerConnection;
            _webOsActionFactory = webOsActionFactory;
            _deviceManager = deviceManager;
            _configuration = configuration;
        }

        public void Initialize()
        {
            var path = GetPath();
            _cache = _fileSerializerConnection.Read<List<RemoteButton>>(path) ?? new List<RemoteButton>();
            
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

            var path = GetPath();
            _fileSerializerConnection.Write(_cache, path);
        }

        private string GetPath()
        {
            var dir = _configuration.GetValue<string>("SettingsPath");
            var path = Path.Combine(dir, Filename);
            return path;
        }
    }
}