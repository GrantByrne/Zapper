using System.Threading.Tasks;
using Zapper.WebOs.Responses.Apps;

namespace Zapper.WebOs.Services
{
    public interface IAppsService
    {
        Task CloseAsync(string appId);
        Task<string> GetCurrentAsync();
        Task LaunchAsync(string appId);
        Task LaunchBrowserAsync(string url);
        Task LaunchYouTubeVideoAsync(string videoId);
        Task<ListLaunchPointsResponse.LaunchPoint[]> ListAsync();
    }
}