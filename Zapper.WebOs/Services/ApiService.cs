using System.Threading.Tasks;
using Zapper.WebOs.Commands.Api;
using Zapper.WebOs.Responses.Api;

namespace Zapper.WebOs.Services
{
    public class ApiService : IApiService
    {
        private readonly IClient _client;

        internal ApiService(IClient client)
        {
            _client = client;
        }

        public async Task<ServiceListResponse.ServiceItem[]> ListServicesAsync()
        {
            var response = await _client.SendCommandAsync<ServiceListResponse>(new ServiceListGetCommand());
            return response.Services;
        }
    }
}
