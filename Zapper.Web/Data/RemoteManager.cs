using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly IRemoteEventHandler _remoteEventHandler;
        private readonly IWebOsActionFactory _webOsActionFactory;
        private readonly IDeviceManager _deviceManager;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        private List<RemoteButton> _cache;

        public RemoteManager(
            IRemoteEventHandler remoteEventHandler,
            IWebOsActionFactory webOsActionFactory,
            IDeviceManager deviceManager,
            IServiceScopeFactory serviceScopeFactory)
        {
            _remoteEventHandler = remoteEventHandler;
            _webOsActionFactory = webOsActionFactory;
            _deviceManager = deviceManager;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public void Initialize()
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ZapperDbContext>();

            _cache = db.RemoteButtons.ToList();

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
            
            using var scope = _serviceScopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ZapperDbContext>();
            db.RemoteButtons.RemoveRange(db.RemoteButtons);
            db.RemoteButtons.AddRange(_cache);
            db.SaveChanges();
        }
    }
}