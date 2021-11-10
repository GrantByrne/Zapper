namespace Zapper.Core.Repository
{
    public interface IFileSerializerConnection
    {
        T Read<T>(string path);

        void Write<T>(T data, string path);
    }
}