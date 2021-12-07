using System.Threading.Tasks;

namespace Zapper.WebOs.Services
{
    public interface IControlService
    {
        Task SendIntentAsync(ControlService.ControlIntent intent);
    }
}