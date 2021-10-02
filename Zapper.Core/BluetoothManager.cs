using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HashtagChris.DotNetBlueZ;
using HashtagChris.DotNetBlueZ.Extensions;

namespace Zapper.Core
{
    public class BluetoothManager : IDisposable
    {
        private Adapter _adapter;
        
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