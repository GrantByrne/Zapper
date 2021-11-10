using System.IO;
using System.Text.Json;

namespace Zapper.Core.Repository
{
    public class FileSerializerConnection : IFileSerializerConnection
    {
        public T Read<T>(string path)
        {
            if (!File.Exists(path))
            {
                return default;
            }
            
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