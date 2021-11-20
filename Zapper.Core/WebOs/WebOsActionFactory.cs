using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WebOsTv.Net;
using WebOsTv.Net.Services;
using Zapper.Core.WebOs.Abstract;
using Zapper.Core.Devices;

namespace Zapper.Core.WebOs
{
    public class WebOsActionFactory : IWebOsActionFactory
    {
        private readonly IWebOsConnectionFactory _webOsConnectionFactory;
        private readonly IWebOsStatusManager _webOsStatusManager;
        private readonly IWakeOnLanManager _wakeOnLanManager;
        private readonly ILogger<WebOsActionFactory> _logger;

        public WebOsActionFactory(
            IWebOsConnectionFactory webOsConnectionFactory,
            IWebOsStatusManager webOsStatusManager,
            IWakeOnLanManager wakeOnLanManager,
            ILogger<WebOsActionFactory> logger)
        {
            _webOsConnectionFactory = webOsConnectionFactory;
            _webOsStatusManager = webOsStatusManager;
            _wakeOnLanManager = wakeOnLanManager;
            _logger = logger;
        }

        public Action Build(string name, Device device)
        {
            var action = name switch
            {
                WebOsActionKey.Mute =>
                    MakeAction(WebOsActionKey.Mute, device, (s, _) => s.Audio.MuteAsync()),
                WebOsActionKey.Unmute =>
                    MakeAction(WebOsActionKey.Unmute, device, (s, _) => s.Audio.UnmuteAsync()),
                WebOsActionKey.VolumeUp =>
                    MakeAction(WebOsActionKey.VolumeUp, device, (s, _) => s.Audio.VolumeUpAsync()),
                WebOsActionKey.VolumeDown =>
                    MakeAction(WebOsActionKey.VolumeDown, device, (s, _) => s.Audio.VolumeDownAsync()),
                WebOsActionKey.ChannelDown =>
                    MakeAction(WebOsActionKey.ChannelDown, device, (s, _) => s.Tv.ChannelDownAsync()),
                WebOsActionKey.ChannelUp =>
                    MakeAction(WebOsActionKey.ChannelUp, device, (s, _) => s.Tv.ChannelUpAsync()),
                WebOsActionKey.TurnOn3d =>
                    MakeAction(WebOsActionKey.TurnOn3d, device, (s, _) => s.Tv.TurnOn3dAsync()),
                WebOsActionKey.TurnOff3d =>
                    MakeAction(WebOsActionKey.TurnOff3d, device, (s, _) => s.Tv.TurnOff3dAsync()),
                WebOsActionKey.Home =>
                    MakeAction(WebOsActionKey.Home, device,
                        (s, _) => s.Control.SendIntentAsync(ControlService.ControlIntent.Home)),
                WebOsActionKey.Back =>
                    MakeAction(WebOsActionKey.Back, device,
                        (s, _) => s.Control.SendIntentAsync(ControlService.ControlIntent.Back)),
                WebOsActionKey.Up =>
                    MakeAction(WebOsActionKey.Up, device,
                        (s, _) => s.Control.SendIntentAsync(ControlService.ControlIntent.Up)),
                WebOsActionKey.Down =>
                    MakeAction(WebOsActionKey.Down, device,
                        (s, _) => s.Control.SendIntentAsync(ControlService.ControlIntent.Down)),
                WebOsActionKey.Left =>
                    MakeAction(WebOsActionKey.Left, device,
                        (s, _) => s.Control.SendIntentAsync(ControlService.ControlIntent.Left)),
                WebOsActionKey.Right =>
                    MakeAction(WebOsActionKey.Right, device,
                        (s, _) => s.Control.SendIntentAsync(ControlService.ControlIntent.Right)),
                WebOsActionKey.Red =>
                    MakeAction(WebOsActionKey.Red, device,
                        (s, _) => s.Control.SendIntentAsync(ControlService.ControlIntent.Red)),
                WebOsActionKey.Blue =>
                    MakeAction(WebOsActionKey.Blue, device,
                        (s, _) => s.Control.SendIntentAsync(ControlService.ControlIntent.Blue)),
                WebOsActionKey.Yellow =>
                    MakeAction(WebOsActionKey.Yellow, device,
                        (s, _) => s.Control.SendIntentAsync(ControlService.ControlIntent.Yellow)),
                WebOsActionKey.Green =>
                    MakeAction(WebOsActionKey.Green, device,
                        (s, _) => s.Control.SendIntentAsync(ControlService.ControlIntent.Green)),
                WebOsActionKey.FastForward =>
                    MakeAction(WebOsActionKey.FastForward, device,
                        (s, _) => s.Control.SendIntentAsync(ControlService.ControlIntent.FastForward)),
                WebOsActionKey.Pause =>
                    MakeAction(WebOsActionKey.Pause, device,
                        (s, _) => s.Control.SendIntentAsync(ControlService.ControlIntent.Pause)),
                WebOsActionKey.Play =>
                    MakeAction(WebOsActionKey.Play, device,
                        (s, _) => s.Control.SendIntentAsync(ControlService.ControlIntent.Play)),
                WebOsActionKey.Rewind =>
                    MakeAction(WebOsActionKey.Rewind, device,
                        (s, _) => s.Control.SendIntentAsync(ControlService.ControlIntent.Rewind)),
                WebOsActionKey.Stop =>
                    MakeAction(WebOsActionKey.Stop, device,
                        (s, _) => s.Control.SendIntentAsync(ControlService.ControlIntent.Stop)),
                WebOsActionKey.PowerOff =>
                    MakeAction(WebOsActionKey.PowerOff, device,
                        (s, _) => s.Control.SendIntentAsync(ControlService.ControlIntent.PowerOff)),
                WebOsActionKey.PowerOn =>
                    MakeAction(WebOsActionKey.PowerOn, device, WakeOnLan),
                WebOsActionKey.ToggleOnOff =>
                    MakeAction(WebOsActionKey.ToggleOnOff, device, ToggleOnOff),
                WebOsActionKey.Enter =>
                    MakeAction(WebOsActionKey.Enter, device,
                        (s, _) => s.Control.SendIntentAsync(ControlService.ControlIntent.Enter)),
                WebOsActionKey.Dash =>
                    MakeAction(WebOsActionKey.Dash, device,
                        (s, _) => s.Control.SendIntentAsync(ControlService.ControlIntent.Dash)),
                WebOsActionKey.Info =>
                    MakeAction(WebOsActionKey.Info, device,
                        (s, _) => s.Control.SendIntentAsync(ControlService.ControlIntent.Info)),
                WebOsActionKey.One =>
                    MakeAction(WebOsActionKey.One, device,
                        (s, _) => s.Control.SendIntentAsync(ControlService.ControlIntent.One)),
                WebOsActionKey.Two =>
                    MakeAction(WebOsActionKey.Two, device,
                        (s, _) => s.Control.SendIntentAsync(ControlService.ControlIntent.Two)),
                WebOsActionKey.Three =>
                    MakeAction(WebOsActionKey.Three, device,
                        (s, _) => s.Control.SendIntentAsync(ControlService.ControlIntent.Three)),
                WebOsActionKey.Four =>
                    MakeAction(WebOsActionKey.Four, device,
                        (s, _) => s.Control.SendIntentAsync(ControlService.ControlIntent.Four)),
                WebOsActionKey.Five =>
                    MakeAction(WebOsActionKey.Five, device,
                        (s, _) => s.Control.SendIntentAsync(ControlService.ControlIntent.Five)),
                WebOsActionKey.Six =>
                    MakeAction(WebOsActionKey.Six, device,
                        (s, _) => s.Control.SendIntentAsync(ControlService.ControlIntent.Six)),
                WebOsActionKey.Seven =>
                    MakeAction(WebOsActionKey.Seven, device,
                        (s, _) => s.Control.SendIntentAsync(ControlService.ControlIntent.Seven)),
                WebOsActionKey.Eight =>
                    MakeAction(WebOsActionKey.Eight, device,
                        (s, _) => s.Control.SendIntentAsync(ControlService.ControlIntent.Eight)),
                WebOsActionKey.Nine =>
                    MakeAction(WebOsActionKey.Nine, device,
                        (s, _) => s.Control.SendIntentAsync(ControlService.ControlIntent.Nine)),
                WebOsActionKey.Zero =>
                    MakeAction(WebOsActionKey.Zero, device,
                        (s, _) => s.Control.SendIntentAsync(ControlService.ControlIntent.Zero)),
                WebOsActionKey.Asterisk =>
                    MakeAction(WebOsActionKey.Asterisk, device,
                        (s, _) => s.Control.SendIntentAsync(ControlService.ControlIntent.Asterisk)),
                WebOsActionKey.Cc =>
                    MakeAction(WebOsActionKey.Cc, device,
                        (s, _) => s.Control.SendIntentAsync(ControlService.ControlIntent.Cc)),
                WebOsActionKey.Exit =>
                    MakeAction(WebOsActionKey.Exit, device,
                        (s, _) => s.Control.SendIntentAsync(ControlService.ControlIntent.Exit)),
                WebOsActionKey.Menu =>
                    MakeAction(WebOsActionKey.Menu, device,
                        (s, _) => s.Control.SendIntentAsync(ControlService.ControlIntent.Menu)),
                _ => throw new ArgumentException("Unsupported Web OS Action")
            };

            return action;
        }

        private async Task ToggleOnOff(IService s, Device d)
        {
            var status = _webOsStatusManager.GetStatus(d.Id);

            if (status == WebOsStatus.On)
            {
                await s.Control.SendIntentAsync(ControlService.ControlIntent.PowerOff);
            }
            else
            {
                _wakeOnLanManager.Send(d.IpAddress, d.MacAddress);
            }
        }

        private Task WakeOnLan(IService s, Device d)
        {
            _wakeOnLanManager.Send(d.IpAddress, d.MacAddress);
            return Task.CompletedTask;
        }

        private Action MakeAction(string name, Device device, Func<IService, Device, Task> action)
        {
            async void Action()
            {
                try
                {
                    _logger.LogInformation($"Running the WebOS command: {name}");

                    var service = await _webOsConnectionFactory.Get(device.IpAddress);

                    await action(service, device);

                    _logger.LogInformation($"Completed running the WebOS command: {name}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to completed WebOS command: {name}");
                }
            }

            var a = new Action(Action);

            return a;
        }
    }
}