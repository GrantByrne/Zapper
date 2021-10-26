using System.Threading.Tasks;
using WebOsTv.Net;

namespace Zapper.Core.WebOs
{
    public interface IWebOsConnectionFactory
    {
        Task<IService> Get(string url = "192.168.1.193");
    }
}