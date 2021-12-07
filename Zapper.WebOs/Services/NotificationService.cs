using System.Threading.Tasks;
using Zapper.WebOs.Commands.Notifications;
using Zapper.WebOs.Responses.Notifications;

namespace Zapper.WebOs.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IClient _client;

        internal NotificationService(IClient client)
        {
            _client = client;
        }

        public async Task SendToastAsync(string message)
        {
            await _client.SendCommandAsync<ToastResponse>(new ToastCommand { Message = message});
        }

    }
}