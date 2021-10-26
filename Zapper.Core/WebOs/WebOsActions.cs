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

            AddAction(WebOsActionKey.Mute, service.Audio.MuteAsync);
            AddAction(WebOsActionKey.Unmute, service.Audio.UnmuteAsync);
            AddAction(WebOsActionKey.VolumeUp, () => service.Audio.VolumeUpAsync());
            AddAction(WebOsActionKey.VolumeDown, () => service.Audio.VolumeDownAsync());
            AddAction(WebOsActionKey.ChannelDown, service.Tv.ChannelDownAsync);
            AddAction(WebOsActionKey.ChannelUp, service.Tv.ChannelUpAsync);
            AddAction(WebOsActionKey.TurnOn3d, service.Tv.TurnOn3dAsync);
            AddAction(WebOsActionKey.TurnOff3d, service.Tv.TurnOff3dAsync);
            AddAction(WebOsActionKey.Home, () => service.Control.SendIntentAsync(ControlService.ControlIntent.Home));
            AddAction(WebOsActionKey.Back, () => service.Control.SendIntentAsync(ControlService.ControlIntent.Back));
            AddAction(WebOsActionKey.Up, () => service.Control.SendIntentAsync(ControlService.ControlIntent.Up));
            AddAction(WebOsActionKey.Down, () => service.Control.SendIntentAsync(ControlService.ControlIntent.Down));
            AddAction(WebOsActionKey.Left, () => service.Control.SendIntentAsync(ControlService.ControlIntent.Left));
            AddAction(WebOsActionKey.Right, () => service.Control.SendIntentAsync(ControlService.ControlIntent.Right));
            AddAction(WebOsActionKey.Red, () => service.Control.SendIntentAsync(ControlService.ControlIntent.Red));
            AddAction(WebOsActionKey.Blue, () => service.Control.SendIntentAsync(ControlService.ControlIntent.Blue));
            AddAction(WebOsActionKey.Yellow, () => service.Control.SendIntentAsync(ControlService.ControlIntent.Yellow));
            AddAction(WebOsActionKey.Green, () => service.Control.SendIntentAsync(ControlService.ControlIntent.Green));
            AddAction(WebOsActionKey.FastForward, () => service.Control.SendIntentAsync(ControlService.ControlIntent.FastForward));
            AddAction(WebOsActionKey.Pause, () => service.Control.SendIntentAsync(ControlService.ControlIntent.Pause));
            AddAction(WebOsActionKey.Play, () => service.Control.SendIntentAsync(ControlService.ControlIntent.Play));
            AddAction(WebOsActionKey.Rewind, () => service.Control.SendIntentAsync(ControlService.ControlIntent.Rewind));
            AddAction(WebOsActionKey.Stop, () => service.Control.SendIntentAsync(ControlService.ControlIntent.Stop));
            AddAction(WebOsActionKey.PowerOff, () => service.Control.SendIntentAsync(ControlService.ControlIntent.PowerOff));
        }
        
        public IEnumerable<string> GetAll()
        {
            return _actions.Keys;
        }

        public Action Get(string key)
        {
            _actions.TryGetValue(key, out var action);
            return action;
        }

        public void Dispose()
        {
            _service?.Close();
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
    }
}