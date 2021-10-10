using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Zapper.Core
{
    public class Lol : IDisposable
    {
        private List<DeviceInputReader> _readers = new();
        
        public event DeviceInputReader.RaiseKeyPress OnKeyPress;

        public Lol()
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
    
    public class DeviceInputReader : IDisposable
    {
        public delegate void RaiseKeyPress(KeyPressEvent e);

        public event RaiseKeyPress OnKeyPress;

        private const int BufferLength = 24;
        
        private readonly byte[] _buffer = new byte[BufferLength];
        
        private FileStream _stream;

        private bool _disposing;

        public DeviceInputReader(string path)
        {
            _stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            Task.Run(Run);
        }

        private void Run()
        {
            while (true)
            {
                if (_disposing)
                {
                    break;
                }

                _stream.Read(_buffer, 0, BufferLength);

                var type = BitConverter.ToInt16(new[] {_buffer[16], _buffer[17]}, 0);
                var code = BitConverter.ToInt16(new[] {_buffer[18], _buffer[19]}, 0);
                var value = BitConverter.ToInt32(new[] {_buffer[20], _buffer[21], _buffer[22], _buffer[23]}, 0);

                if (type == 1)
                {
                    var c = (Keycode) code;
                    var s = (KeyState) value;
                    var e = new KeyPressEvent(c, s);

                    OnKeyPress?.Invoke(e);
                }
            }
        }

        public void Dispose()
        {
            _disposing = true;
            _stream.Dispose();
            _stream = null;
        }
    }
}