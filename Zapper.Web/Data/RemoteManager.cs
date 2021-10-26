using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Zapper.Core;
using Zapper.Core.WebOs;
using Zapper.Web.Data.Abstract;

namespace Zapper.Web.Data
{
    public class RemoteManager : IRemoteManager
    {
        private readonly IRemoteEventHandler _remoteEventHandler;
        private readonly IWebOsActions _webOsActions;
        private readonly ILogger<RemoteManager> _logger;

        private List<RemoteButton> _cache = new();

        public RemoteManager(
            IRemoteEventHandler remoteEventHandler,
            IWebOsActions webOsActions,
            ILogger<RemoteManager> logger)
        {
            _remoteEventHandler = remoteEventHandler;
            _webOsActions = webOsActions;
            _logger = logger;
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
        }
    }
}