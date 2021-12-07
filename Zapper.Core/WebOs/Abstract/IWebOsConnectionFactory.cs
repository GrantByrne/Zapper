using System.Threading.Tasks;
using Zapper.WebOs;

namespace Zapper.Core.WebOs.Abstract
{
    public interface IWebOsConnectionFactory
    {
        Task<IService> Get(string url = "192.168.1.193");
    }
}