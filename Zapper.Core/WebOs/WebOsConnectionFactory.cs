using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WebOsTv.Net;

namespace Zapper.Core.WebOs
{
    public class WebOsConnectionFactory : IWebOsConnectionFactory
    {
        private readonly ILogger<WebOsConnectionFactory> _logger;
        private readonly Dictionary<string, IService> _services = new();
        private bool _connecting;

        public WebOsConnectionFactory(ILogger<WebOsConnectionFactory> logger)
        {
            _logger = logger;
        }

        public async Task<IService> Get(string url = "192.168.1.193")
        {
            if (_services.TryGetValue(url, out var service))
                return service;
            
            _logger.LogInformation($"WebOS connection not found. Attempting to connect to {url}");

            if (_connecting)
                return null;

            _connecting = true;
            
            service = new Service();

            try
            {
                await service.ConnectAsync(url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to WebOS TV");
            }
            finally
            {
                _connecting = false;
            }
            
            _services[url] = service;
            
            _logger.LogInformation($"Successfully connected to {url}");

            return service;
        }
    }
}