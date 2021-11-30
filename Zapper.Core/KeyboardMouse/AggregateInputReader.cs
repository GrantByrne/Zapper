using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using Microsoft.Extensions.Logging;
using Zapper.Core.KeyboardMouse.Abstract;

namespace Zapper.Core.KeyboardMouse
{
    public class AggregateInputReader : IDisposable, IAggregateInputReader
    {
        private readonly ILogger<InputReader> _inputReaderLogger;
        
        private Dictionary<string, InputReader> _readers = new();
        
        public event InputReader.RaiseKeyPress OnKeyPress;

        public AggregateInputReader(ILogger<InputReader> inputReaderLogger)
        {
            _inputReaderLogger = inputReaderLogger;

            var timer = new Timer();
            timer.Interval = 10 * 1000;
            timer.Enabled = true;
            timer.Elapsed += (_, _) => Scan(); 
            timer.Start();
        }

        private void ReaderOnOnKeyPress(KeyPressEvent e)
        {
            OnKeyPress?.Invoke(e);
        }

        private void Scan()
        {
            var files = Directory.GetFiles("/dev/input/", "event*");

            foreach (var file in files)
            {
                if (_readers.ContainsKey(file))
                {
                    continue;
                }
                
                var reader = new InputReader(file, _inputReaderLogger);
                
                reader.OnKeyPress += ReaderOnOnKeyPress;

                _readers.Add(file, reader);
            }

            var deadReaders = _readers.Values.Where(r => r.Faulted);

            foreach (var dr in deadReaders)
            {
                _readers.Remove(dr.Path);
                dr.OnKeyPress -= ReaderOnOnKeyPress;
                dr.Dispose();
            }
        }

        public void Dispose()
        {
            foreach (var d in _readers.Values)
            {
                d.OnKeyPress -= ReaderOnOnKeyPress;
                d.Dispose();
            }

            _readers = null;
        }
    }
}