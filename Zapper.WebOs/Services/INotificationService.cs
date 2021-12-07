using System.Threading.Tasks;

namespace Zapper.WebOs.Services
{
    public interface INotificationService
    {
        Task SendToastAsync(string message);
    }
}