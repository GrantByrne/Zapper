using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Zapper.Core.Mouse
{
    public class MouseInputReader : IDisposable
    {
        private const int BufferLength = 4;
        
        private readonly byte[] _buffer = new byte[BufferLength];
        
        private FileStream _stream;
        private bool _disposing;
        
        public MouseInputReader(string path)
        {
            _stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            
            Task.Run(Run);
        }

        private void Run()
        {
            while (true)
            {
                if (_disposing)
                    break;
                
                _stream.Read(_buffer, 0, BufferLength);
                
                var left = BitConverter.ToBoolean(new[] {_buffer[0], _buffer[0]}, 0);
                var middle = BitConverter.ToBoolean(new[] {_buffer[1], _buffer[1]}, 0);
                var right = BitConverter.ToBoolean(new[] {_buffer[2], _buffer[2]}, 0);
                
                // TODO - Add in X & Y changes
                // TODO - Add in Scroll changes

                var sb = new StringBuilder();

                foreach (var b in _buffer)
                {
                    var str = Convert.ToString(b, 2).PadLeft(8,'0');
                    sb.Append(str);
                    sb.Append(" ");
                }
                
                Console.WriteLine(sb.ToString());
            }   
        }

        public void Dispose()
        {
            _disposing = true;
            
            if (_stream == null) 
                return;
            
            _stream.Dispose();
            _stream = null;
        }
    }
}