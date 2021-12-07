using System.Threading.Tasks;
using Zapper.WebOs.Responses.Api;

namespace Zapper.WebOs.Services
{
    public interface IApiService
    {
        Task<ServiceListResponse.ServiceItem[]> ListServicesAsync();
    }
}