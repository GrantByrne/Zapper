using System.Threading.Tasks;
using Zapper.WebOs.Commands;
using Zapper.WebOs.Commands.Tv;
using Zapper.WebOs.Responses;

namespace Zapper.WebOs
{
    public interface IClient
    {
        Task ConnectAsync(string hostName);
        Task<TResponse> SendCommandAsync<TResponse>(CommandBase command) where TResponse : ResponseBase;
        Task SendButtonAsync(ButtonType type);
        void Close();
    }
}