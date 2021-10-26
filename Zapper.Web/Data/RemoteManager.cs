using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Zapper.Core;
using Zapper.Web.Data.Abstract;

namespace Zapper.Web.Data
{
    public class RemoteManager : IRemoteManager
    {
        private readonly IRemoteEventHandler _remoteEventHandler;
        private readonly ILogger<RemoteManager> _logger;

        private List<RemoteButton> _cache = new();

        public RemoteManager(
            IRemoteEventHandler remoteEventHandler,
            ILogger<RemoteManager> logger)
        {
            _remoteEventHandler = remoteEventHandler;
            _logger = logger;
        }

        public IEnumerable<RemoteButton> Get()
        {
            return _cache;
        }

        public void Update(IEnumerable<RemoteButton> remoteButtons)
        {
            _remoteEventHandler.ClearActions();

            foreach (var remote in remoteButtons)
            {
                _remoteEventHandler.RegisterAction(remote.Code, () => { _logger.LogInformation($"{remote.Name} Button was pressed"); });   
            }
            
            _cache = remoteButtons.ToList();
        }
    }
}