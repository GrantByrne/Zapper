using System.Net;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Logging;
using Zapper.Core.WebOs.Abstract;

namespace Zapper.Core.WebOs
{
    public class WakeOnLanManager : IWakeOnLanManager
    {
        private readonly ILogger<WakeOnLanManager> _logger;

        public WakeOnLanManager(ILogger<WakeOnLanManager> logger)
        {
            _logger = logger;
        }
        
        public void Send(string ipAddress, string macAddress)
        {
            _logger.LogInformation($"Sending Wake On Lan Packet to: {macAddress}");
            
            PhysicalAddress.Parse(macAddress).SendWol();
            
            _logger.LogInformation($"Sent Wake On Lan Packet to: {macAddress}");
        }
    }
}