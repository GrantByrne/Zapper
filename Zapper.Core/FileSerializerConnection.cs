using System.IO;
using System.Text.Json;

namespace Zapper.Core
{
    public class FileSerializerConnection : IFileSerializerConnection
    {
        public T Read<T>(string path)
        {
            var json = File.ReadAllText(path);
            var result = JsonSerializer.Deserialize<T>(json);
            return result;
        }

        public void Write<T>(T data, string path)
        {
            var json = JsonSerializer.Serialize(data);
            File.WriteAllText(path, json);
        }
    }
}