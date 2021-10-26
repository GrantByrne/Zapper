namespace Zapper.Core
{
    public interface IFileSerializerConnection
    {
        T Read<T>(string path);

        void Write<T>(T data, string path);
    }
}