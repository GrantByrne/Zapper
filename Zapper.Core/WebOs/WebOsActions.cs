using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WebOsTv.Net;
using WebOsTv.Net.Services;

namespace Zapper.Core.WebOs
{
    public class WebOsActions : IWebOsActions, IDisposable
    {
        private readonly IService _service;
        private readonly ILogger<WebOsActions> _logger;
        private readonly Dictionary<string, Action> _actions = new();

        public WebOsActions(
            IService service,
            ILogger<WebOsActions> logger)
        {
            _service = service;
            _logger = logger;
            service.ConnectAsync("192.168.1.100");

            AddAction("Mute", service.Audio.MuteAsync);
            AddAction("Unmute", service.Audio.UnmuteAsync);
            AddAction("Volume Up", () => service.Audio.VolumeUpAsync());
            AddAction("Volume Down", () => service.Audio.VolumeDownAsync());
            AddAction("Channel Down", service.Tv.ChannelDownAsync);
            AddAction("Channel Up", service.Tv.ChannelUpAsync);
            AddAction("Turn On", service.Tv.TurnOn3dAsync);
            AddAction("Turn Off", service.Tv.TurnOff3dAsync);
            AddAction("Home", () => service.Control.SendIntentAsync(ControlService.ControlIntent.Home));
            AddAction("Back", () => service.Control.SendIntentAsync(ControlService.ControlIntent.Back));
            AddAction("Up", () => service.Control.SendIntentAsync(ControlService.ControlIntent.Up));
            AddAction("Down", () => service.Control.SendIntentAsync(ControlService.ControlIntent.Down));
            AddAction("Left", () => service.Control.SendIntentAsync(ControlService.ControlIntent.Left));
            AddAction("Right", () => service.Control.SendIntentAsync(ControlService.ControlIntent.Right));
            AddAction("Red", () => service.Control.SendIntentAsync(ControlService.ControlIntent.Red));
            AddAction("Blue", () => service.Control.SendIntentAsync(ControlService.ControlIntent.Blue));
            AddAction("Yellow", () => service.Control.SendIntentAsync(ControlService.ControlIntent.Yellow));
            AddAction("Green", () => service.Control.SendIntentAsync(ControlService.ControlIntent.Green));
            AddAction("FastForward", () => service.Control.SendIntentAsync(ControlService.ControlIntent.FastForward));
            AddAction("Pause", () => service.Control.SendIntentAsync(ControlService.ControlIntent.Pause));
            AddAction("Play", () => service.Control.SendIntentAsync(ControlService.ControlIntent.Play));
            AddAction("Rewind", () => service.Control.SendIntentAsync(ControlService.ControlIntent.Rewind));
            AddAction("Stop", () => service.Control.SendIntentAsync(ControlService.ControlIntent.Stop));
            AddAction("PowerOff", () => service.Control.SendIntentAsync(ControlService.ControlIntent.PowerOff));
        }

        private void AddAction(string name, Func<Task> action)
        {
            async void Action()
            {
                try
                {
                    _logger.LogInformation($"Running the WebOS command: {name}");
                    await action();
                    _logger.LogInformation($"Completed running the WebOS command: {name}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to completed WebOS command: {name}");
                }
            }

            _actions.Add(name, Action);
        }

        public IEnumerable<string> GetAll()
        {
            return _actions.Keys;
        }

        public void Dispose()
        {
            _service?.Close();
        }
    }
}