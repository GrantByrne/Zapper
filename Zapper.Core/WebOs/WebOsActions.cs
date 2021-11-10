using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WebOsTv.Net;
using WebOsTv.Net.Services;
using Zapper.Core.WebOs.Abstract;

namespace Zapper.Core.WebOs
{
    public class WebOsActions : IWebOsActions
    {
        private readonly IWebOsConnectionFactory _webOsConnectionFactory;
        private readonly ILogger<WebOsActions> _logger;
        private readonly IWebOsStatusManager _webOsStatusManager;
        private readonly Dictionary<string, Action> _actions = new();

        public WebOsActions(
            IWebOsConnectionFactory webOsConnectionFactory,
            ILogger<WebOsActions> logger,
            IWebOsStatusManager webOsStatusManager)
        {
            _webOsConnectionFactory = webOsConnectionFactory;
            _logger = logger;
            _webOsStatusManager = webOsStatusManager;

            AddAction(WebOsActionKey.Mute, service => service.Audio.MuteAsync());
            AddAction(WebOsActionKey.Unmute, service => service.Audio.UnmuteAsync());
            AddAction(WebOsActionKey.VolumeUp, service => service.Audio.VolumeUpAsync());
            AddAction(WebOsActionKey.VolumeDown, service => service.Audio.VolumeDownAsync());
            AddAction(WebOsActionKey.ChannelDown, service => service.Tv.ChannelDownAsync());
            AddAction(WebOsActionKey.ChannelUp, service => service.Tv.ChannelUpAsync());
            AddAction(WebOsActionKey.TurnOn3d, service => service.Tv.TurnOn3dAsync());
            AddAction(WebOsActionKey.TurnOff3d, service => service.Tv.TurnOff3dAsync());
            AddAction(WebOsActionKey.Home, service => service.Control.SendIntentAsync(ControlService.ControlIntent.Home));
            AddAction(WebOsActionKey.Back, service => service.Control.SendIntentAsync(ControlService.ControlIntent.Back));
            AddAction(WebOsActionKey.Up, service => service.Control.SendIntentAsync(ControlService.ControlIntent.Up));
            AddAction(WebOsActionKey.Down, service => service.Control.SendIntentAsync(ControlService.ControlIntent.Down));
            AddAction(WebOsActionKey.Left, service => service.Control.SendIntentAsync(ControlService.ControlIntent.Left));
            AddAction(WebOsActionKey.Right, service =>service.Control.SendIntentAsync(ControlService.ControlIntent.Right));
            AddAction(WebOsActionKey.Red, service => service.Control.SendIntentAsync(ControlService.ControlIntent.Red));
            AddAction(WebOsActionKey.Blue, service => service.Control.SendIntentAsync(ControlService.ControlIntent.Blue));
            AddAction(WebOsActionKey.Yellow, service => service.Control.SendIntentAsync(ControlService.ControlIntent.Yellow));
            AddAction(WebOsActionKey.Green, service => service.Control.SendIntentAsync(ControlService.ControlIntent.Green));
            AddAction(WebOsActionKey.FastForward, service => service.Control.SendIntentAsync(ControlService.ControlIntent.FastForward));
            AddAction(WebOsActionKey.Pause, service => service.Control.SendIntentAsync(ControlService.ControlIntent.Pause));
            AddAction(WebOsActionKey.Play, service => service.Control.SendIntentAsync(ControlService.ControlIntent.Play));
            AddAction(WebOsActionKey.Rewind, service => service.Control.SendIntentAsync(ControlService.ControlIntent.Rewind));
            AddAction(WebOsActionKey.Stop, service => service.Control.SendIntentAsync(ControlService.ControlIntent.Stop));
            AddAction(WebOsActionKey.PowerOff, service => service.Control.SendIntentAsync(ControlService.ControlIntent.PowerOff));
            AddAction(WebOsActionKey.PowerOn, PowerOn);
            AddAction(WebOsActionKey.ToggleOnOff, ToggleOnOff);
        }

        private Task PowerOn(IService arg)
        {
            // TODO - Send a wake on lan packet
            throw new NotImplementedException();
        }

        private Task ToggleOnOff(IService arg)
        {
            // TODO - We'll need to figure out a way to get the device id to figure out which Web OS tv to turn on/on
            throw new NotImplementedException();
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

        private void AddAction(string name, Func<IService, Task> action)
        {
            async void Action()
            {
                try
                {
                    _logger.LogInformation($"Running the WebOS command: {name}");
                    
                    var service = await _webOsConnectionFactory.Get();
                    
                    await action(service);
                    
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