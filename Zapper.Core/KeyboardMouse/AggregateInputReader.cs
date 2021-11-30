using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using Zapper.Core.KeyboardMouse.Abstract;

namespace Zapper.Core.KeyboardMouse
{
    public class AggregateInputReader : IDisposable, IAggregateInputReader
    {
        private List<InputReader> _readers = new();
        
        public event InputReader.RaiseKeyPress OnKeyPress;

        public AggregateInputReader(ILogger<InputReader> inputReaderLogger)
        {
            var files = Directory.GetFiles("/dev/input/", "event*");

            foreach (var file in files)
            {
                var reader = new InputReader(file, inputReaderLogger);
                
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