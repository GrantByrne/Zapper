using System.Threading.Tasks;
using WebOsTv.Net;
using WebOsTv.Net.Services;
using Zapper.Core;
using Zapper.Core.KeyboardMouse;

namespace Zapper.Console
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var service = new Service();

            await service.ConnectAsync("192.168.1.193");
            
            var handler = new RemoteEventHandler();
            
            handler.RegisterAction(EventCode.Down, async () =>
            {
                await service.Control.SendIntentAsync(ControlService.ControlIntent.Down);
            });
            
            handler.RegisterAction(EventCode.Up, async () =>
            {
                await service.Control.SendIntentAsync(ControlService.ControlIntent.Up);
            });
            
            handler.RegisterAction(EventCode.Left, async () =>
            {
                await service.Control.SendIntentAsync(ControlService.ControlIntent.Left);
            });
            
            handler.RegisterAction(EventCode.Right, async () =>
            {
                await service.Control.SendIntentAsync(ControlService.ControlIntent.Right);
            });
            
            System.Console.ReadLine();
        }
    }
}