using System;
using System.Collections.Generic;
using WebOsTv.Net;
using WebOsTv.Net.Services;

namespace Zapper.Core.WebOs
{
    public class WebOsActions : IWebOsActions, IDisposable
    {
        private readonly IService _service;
        private readonly Dictionary<string, Action> _actions = new();

        public WebOsActions(IService service)
        {
            _service = service;
            service.ConnectAsync("192.168.1.100");
            
            _actions.Add("Mute", () => { service.Audio.MuteAsync(); } );
            _actions.Add("Unmute", () => { service.Audio.UnmuteAsync(); } );
            _actions.Add("Volume Up", () => { service.Audio.VolumeUpAsync(); } );
            _actions.Add("Volume Down", () => { service.Audio.VolumeDownAsync(); } );
            _actions.Add("Channel Down", () => { service.Tv.ChannelDownAsync(); } );
            _actions.Add("Channel Up", () => { service.Tv.ChannelUpAsync(); } );
            _actions.Add("Turn On 3D", () => { service.Tv.TurnOn3dAsync(); } );
            _actions.Add("Turn Off 3D", () => { service.Tv.TurnOff3dAsync(); } );
            _actions.Add("Home", () => { service.Control.SendIntentAsync(ControlService.ControlIntent.Home); } );
            _actions.Add("Back", () => { service.Control.SendIntentAsync(ControlService.ControlIntent.Back); } );
            _actions.Add("Up", () => { service.Control.SendIntentAsync(ControlService.ControlIntent.Up); } );
            _actions.Add("Down", () => { service.Control.SendIntentAsync(ControlService.ControlIntent.Down); } );
            _actions.Add("Left", () => { service.Control.SendIntentAsync(ControlService.ControlIntent.Left); } );
            _actions.Add("Right", () => { service.Control.SendIntentAsync(ControlService.ControlIntent.Right); } );
            _actions.Add("Red", () => { service.Control.SendIntentAsync(ControlService.ControlIntent.Red); } );
            _actions.Add("Blue", () => { service.Control.SendIntentAsync(ControlService.ControlIntent.Blue); } );
            _actions.Add("Yellow", () => { service.Control.SendIntentAsync(ControlService.ControlIntent.Yellow); } );
            _actions.Add("Green", () => { service.Control.SendIntentAsync(ControlService.ControlIntent.Green); } );
            _actions.Add("FastForward", () => { service.Control.SendIntentAsync(ControlService.ControlIntent.FastForward); } );
            _actions.Add("Pause", () => { service.Control.SendIntentAsync(ControlService.ControlIntent.Pause); } );
            _actions.Add("Play", () => { service.Control.SendIntentAsync(ControlService.ControlIntent.Play); } );
            _actions.Add("Rewind", () => { service.Control.SendIntentAsync(ControlService.ControlIntent.Rewind); } );
            _actions.Add("Stop", () => { service.Control.SendIntentAsync(ControlService.ControlIntent.Stop); } );
            _actions.Add("PowerOff", () => { service.Control.SendIntentAsync(ControlService.ControlIntent.PowerOff); } );
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