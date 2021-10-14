using System;
using System.Collections.Generic;
using System.IO;

namespace Zapper.Core
{
    public class AllDeviceInputManager : IDisposable
    {
        private List<DeviceInputReader> _readers = new();
        
        public event DeviceInputReader.RaiseKeyPress OnKeyPress;

        public AllDeviceInputManager()
        {
            var files = Directory.GetFiles("/dev/input/", "event*");

            foreach (var file in files)
            {
                var reader = new DeviceInputReader(file);
                
                reader.OnKeyPress += ReaderOnOnKeyPress;

                _readers.Add(reader);
            }
        }

        private void ReaderOnOnKeyPress(KeyPressEvent e)
        {
            OnKeyPress?.Invoke(e);
        }

        public void Dispose()
        {
            foreach (var d in _readers)
            {
                d.OnKeyPress -= ReaderOnOnKeyPress;
                d.Dispose();
            }

            _readers = null;
        }
    }
}