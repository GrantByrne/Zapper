using System.Threading.Tasks;

namespace Zapper.WebOs.Auth
{
    public interface IKeyStore
    {
        Task StoreKeyAsync(string hostName, string key);
        Task<string> GetKeyAsync(string hostName);
    }
}
