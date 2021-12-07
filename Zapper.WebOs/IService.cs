using System.Threading.Tasks;
using Zapper.WebOs.Services;

namespace Zapper.WebOs
{
    public interface IService
    {
        IApiService Api { get; }
        IAppsService Apps { get; }
        IAudioService Audio { get; }
        IControlService Control { get; }
        INotificationService Notifications { get; }
        ITvService Tv { get; }
        Task ConnectAsync(string hostName);
        void Close();
    }
}