using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HashtagChris.DotNetBlueZ;
using HashtagChris.DotNetBlueZ.Extensions;
using Microsoft.Extensions.Logging;

namespace Zapper.Core.Bluetooth
{
    public class BluetoothManager : IDisposable
    {
        private readonly ILogger<BluetoothManager> _logger;
        private Adapter _adapter;

        public BluetoothManager(ILogger<BluetoothManager> logger)
        {
            _logger = logger;
        }
        
        public async Task StartUp()
        {
            var adapters = await BlueZManager.GetAdaptersAsync();
            _adapter = adapters.FirstOrDefault();
        }

        public async Task<IEnumerable<string>> GetBluetoothDevices()
        {
            var devices = await _adapter.GetDevicesAsync();

            var names = new List<string>();
            
            foreach (var d in devices)
            {
                string name = null;

                try
                {
                    name = await d.GetNameAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while getting bluetooth devices.");
                }

                if (!string.IsNullOrWhiteSpace(name))
                {
                    names.Add(name);
                }
            }

            return names;
        }

        public void Dispose()
        {
            _adapter?.Dispose();
        }
    }
}