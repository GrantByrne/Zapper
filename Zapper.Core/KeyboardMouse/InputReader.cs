using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Zapper.Core.KeyboardMouse
{
    public class InputReader : IDisposable
    {
        private readonly ILogger<InputReader> _logger;
        private const int BufferLength = 24;
        
        private static readonly int PiOffset;

        private readonly byte[] _buffer = new byte[BufferLength];

        private FileStream _stream;
        private bool _disposing;

        public delegate void RaiseKeyPress(KeyPressEvent e);

        public delegate void RaiseMouseMove(MouseMoveEvent e);

        public event RaiseKeyPress OnKeyPress;

        public event RaiseMouseMove OnMouseMove;
        
        public string Path { get; }
        
        public bool Faulted { get; private set; }

        static InputReader()
        {
            if (RunningOnRaspberryPi())
            {
                PiOffset = -8;
            }
        }

        public InputReader(
            string path,
            ILogger<InputReader> logger)
        {
            _logger = logger;
            
            Path = path;

            try
            {
                _stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            }
            catch (IOException ex)
            {
                _logger.LogWarning(ex, $"Error occurred while trying to build stream for {path}");
                Faulted = true;
            }

            Task.Run(Run);
        }

        private void Run()
        {
            while (true)
            {
                if (_disposing)
                    break;

                try
                {
                    _stream.Read(_buffer, 0, BufferLength);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Error occured while trying to read from the stream for {Path}");
                    Faulted = true;
                }

                var type = GetEventType();
                var code = GetCode();
                var value = GetValue();

                switch (type)
                {
                    case EventType.EV_KEY:
                        HandleKeyPressEvent(code, value);
                        break;
                    case EventType.EV_REL:
                        var axis = (MouseAxis) code;
                        var e = new MouseMoveEvent(axis, value);
                        OnMouseMove?.Invoke(e);
                        break;
                }
            }
        }

        private int GetValue()
        {
            var valueBits = new[]
            {
                _buffer[20 + PiOffset],
                _buffer[21 + PiOffset],
                _buffer[22 + PiOffset],
                _buffer[23 + PiOffset]
            };
            
            var value = BitConverter.ToInt32(valueBits, 0);
            
            return value;
        }

        private short GetCode()
        {
            var codeBits = new[]
            {
                _buffer[18 + PiOffset],
                _buffer[19 + PiOffset]
            };
            
            var code = BitConverter.ToInt16(codeBits, 0);
            
            return code;
        }

        private EventType GetEventType()
        {
            var typeBits = new[]
            {
                _buffer[16 + PiOffset],
                _buffer[17 + PiOffset]
            };

            var type = BitConverter.ToInt16(typeBits, 0);

            var eventType = (EventType) type;
            
            return eventType;
        }

        private void HandleKeyPressEvent(short code, int value)
        {
            var c = (EventCode) code;
            var s = (KeyState) value;
            var e = new KeyPressEvent(c, s);
            OnKeyPress?.Invoke(e);
        }

        public void Dispose()
        {
            _disposing = true;
            _stream.Dispose();
            _stream = null;
        }

        private static bool RunningOnRaspberryPi()
        {
            var path = "/proc/cpuinfo";
            var text = File.ReadAllLines(path);
            var runningOnPi = text.Any(l => l.Contains("Raspberry Pi"));
            return runningOnPi;
        }
    }
}